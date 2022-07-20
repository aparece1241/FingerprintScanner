using System;
using System.IO;
using Enrollment;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Enrollment
{
    public partial class frmVerificar : CaptureFormVerify
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private bool isSomeoneVerified = false;
        private EmployeeModel verifiedEmployee;
        private List<PendingAttendance> pendingAttendances = new List<PendingAttendance> { };

        // test logger
        private List<string> loggerData = new List<string> { };

        DataManager dal;

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            Config objt = new Config();
            this.dal = new DataManager();
            base.Init();
            base.Text = objt.company;
            //Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator
            UpdateStatus(0);
            initFields();
        }

        private void UpdateStatus(int FAR)
        {
            // Show "False accept rate" value
            //SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR));
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            SetPrompt("Processing...");

            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Check quality of the sample and start verification if it's good
            // TODO: move to a separate task
            if (features != null)
            {
                System.Threading.Timer timer = null;

                try
                {
                    IEnumerable<EmployeeModel> employees = new EmployeeModel[] { };
                    if (FileHandler.IsDataLoaded && FileHandler.validateFile((new EmployeeModel()).getFileName()))
                    {
                        employees = FileHandler.retrieveData(new EmployeeModel());
                    }
                    else
                    {
                        // Retrieve attendance data after submit
                        FileHandler.saveData(new AttendanceModel());

                        // Retrieve employee data after submit
                        FileHandler.saveData(new EmployeeModel());

                        employees = dal.getEmployees(1);
                    }


                    // check length the length of employees
                    if (employees.Count() == 0)
                    {
                        SetPrompt("Please Try Again");

                        timer = new System.Threading.Timer((obj) =>
                        {
                            initializeFields();
                            timer.Dispose();
                        }, null, 1000, System.Threading.Timeout.Infinite);

                        return;
                    }

                    // array of tasks
                    List<Task> tasks = new List<Task>();
                    int employeePerTask = 100;
                    double iteration = (employees.Count() / (double)employeePerTask);
                    iteration = Math.Ceiling(iteration);



                    // devide employees
                    for (int i = 0; i < iteration; i++)
                    {
                        EmployeeModel[] emps = employees.Skip(employeePerTask * i).Take(employeePerTask).ToArray();

                        // create tasks
                        Task task = Task.Factory.StartNew(() => this.matchingFingerPrints(emps, features));
                        tasks.Add(task);
                    }
                    
                    // DO NOT DELETE FOR TESTING PURPOSE
                    // comparing with out using threads
                    //this.matchingFingerPrints(employees.ToArray(), features);

                    //wait all task
                    Task.WaitAll(tasks.ToArray());

                    string logData = "[" + DateTime.Now.ToString("hh:mm: ss") + "]: " + JsonConvert.SerializeObject(this.loggerData);
                    string fileName = DateTime.Now.ToString("MM-dd-yyyy") + ".log";
                    Logger.log(LogType.INFO, logData, this.GetType().Name);

                    if (this.loggerData.Count() > 1)
                    {
                        this.loggerData = new List<string>();
                        throw new Exception("Matched two fingerprint!");
                    }

                    this.loggerData = new List<string>();
                    // check if no one is verified
                    if (!this.isSomeoneVerified)
                    {
                        SetPrompt("Please Try Again!");
                    }
                    else
                    {
                        var _name = this.verifiedEmployee.last_name + ", ";
                        _name += this.verifiedEmployee.first_name + " ";
                        _name += this.verifiedEmployee.middle_name;

                        SetName(_name);
                        SaveRecord(this.verifiedEmployee);
                    }

                    // re-initialized isSomeoneVerified by false
                    this.isSomeoneVerified = false;

                    timer = new System.Threading.Timer((obj) =>
                    {
                        initializeFields();
                        timer.Dispose();
                    }, null, 1000, System.Threading.Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Logger.log(LogType.ERROR, ex.ToString(), this.GetType().Name);
                    MessageBox.Show(ex.Message);
                    timer = new System.Threading.Timer((obj) =>
                    {
                        initializeFields();
                        timer.Dispose();
                    }, null, 1000, System.Threading.Timeout.Infinite);
                }
            }
        }

        private void matchingFingerPrints(
            EmployeeModel[] employees,
            DPFP.FeatureSet features
        )
        {
            // Compare the feature set with our template
            DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

            DPFP.Template template = new DPFP.Template();
            Stream stream;

            for (int index = 0; employees.Length > index; index++)
            {
                if (employees[index].status == "" || employees[index].status == null)
                {
                    continue;
                }

                byte[] raw = Convert.FromBase64String(employees[index].status.Replace(" ", "+"));
                stream = new MemoryStream(raw);


                template = new DPFP.Template(stream);
                try
                {
                    Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator

                    Verificator.Verify(features, template, ref result);
                }
                catch (Exception e)
                {
                    Logger.log(LogType.ERROR, e.ToString(), this.GetType().Name);
                    return;
                }
                UpdateStatus(result.FARAchieved);

                if (result.Verified)
                {
                    this.verifiedEmployee = employees[index];
                    this.isSomeoneVerified = true;
                    this.loggerData.Add(employees[index].emp_id + " " + employees[index].full_name + " " + Task.CurrentId.ToString());
                }
            }
        }

        private void initFields()
        {
            initializeFields();
        }

        private void SaveRecord(EmployeeModel employee)
        {
            dal = new DataManager();
            DateTime _datetime = DateTime.Now;

            // update the attendance time in time out
            if (RealTimeHandler.isInternetConnnected) {
                // This line must be used for now
                //SetPrompt(dal.SaveAttendance(employee.id, _datetime));

                if (FileHandler.IsDataLoaded && FileHandler.validateFile((new AttendanceModel()).getFileName()))
                {
                    SetPrompt(dal.FingerprintResponseMsg(employee.id, _datetime));
                    dal.SaveAttendance(employee.id, _datetime);
                }
                else
                {
                    // Retrieve attendance data after submit
                    FileHandler.saveData(new AttendanceModel());

                    // Retrieve employee data after submit
                    FileHandler.saveData(new EmployeeModel());

                    SetPrompt(dal.SaveAttendance(employee.id, _datetime));
                }
            }
            else
            {
                string responseMsg = dal.FingerprintResponseMsg(employee.id, _datetime);
                SetPrompt(responseMsg);
                pendingAttendances = new List<PendingAttendance>(FileHandler.retrieveData(new PendingAttendance(), "pendings"));

                PendingAttendance att = new PendingAttendance();
                att.emp_id = employee.id;
                att.date = _datetime;

                if (!responseMsg.Contains("Too early to"))
                {
                    pendingAttendances.Add(att);
                }

                string serializedData = JsonConvert.SerializeObject(pendingAttendances, Formatting.Indented);
                FileHandler.FingerPrintLogger(serializedData, att.getFileName(), "pendings", false);
            }
        }

        public frmVerificar()
        {
            InitializeComponent();
        }

        private void frmVerificar_Load(object sender, EventArgs e)
        {

        }
    }
}

