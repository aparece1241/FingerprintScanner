using System;
using System.IO;
using Enrollement;
using System.Linq;
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
        private Task taskmatching1;
        private Task taskmatching2;
        private Task taskmatching3;
        private Task taskmatching4;
        private Task taskmatching5;
        private Task taskmatching6;
        private Task taskmatching7;

        DataManager dal;

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            Config objt = new Config();

            base.Init();
            base.Text = objt.company;
            Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator
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
            Console.WriteLine(Convert.ToBase64String(Sample.Bytes));
            if (features != null)
            {
                System.Threading.Timer timer = null;

                // Compare the feature set with our template
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                DPFP.Template template = new DPFP.Template();
                
                try
                {
                    // query all employees from api
                    dal = new DataManager();
                    IEnumerable<EmployeeModel> employees = dal.getEmployee(1);

                    // check length the length of employees
                    if (employees.Count() == 0)
                    {
                        SetPrompt("Please Try Again");

                        timer = new System.Threading.Timer((obj) =>
                        {
                            initializeFields();
                            timer.Dispose();
                        }, null, 2000, System.Threading.Timeout.Infinite);

                        return;
                    }

                    // array of tasks
                    List<Task> tasks = new List<Task>();
                    int employeePerTask = 100;
                    double iteration = (employees.Count() / (double) employeePerTask);
                    iteration = Math.Ceiling(iteration);

                    // devide employees
                    for (int i = 0; i < iteration; i++)
                    {
                        EmployeeModel[] emps = employees.Skip(employeePerTask * i).Take(employeePerTask).ToArray();

                        // create tasks
                        Task task = Task.Factory.StartNew(() => this.matchingFingerPrints(emps, features, template, result));
                        tasks.Add(task);
                    }

                    // comparing with out using threads
                    //this.matchingFingerPrints(employees.ToArray(), features, template, result);

                    // wait all task
                    Task.WaitAll(tasks.ToArray());

                    // check if no one is verified
                    if (!this.isSomeoneVerified)
                    {
                        SetPrompt("Please Try Again!");
                    } else
                    {
                        var _name = this.verifiedEmployee.last_name + ", ";
                        _name += this.verifiedEmployee.first_name;
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
                    }, null, 2000, System.Threading.Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    
        private void matchingFingerPrints(
            EmployeeModel[] employees,
            DPFP.FeatureSet features,
            DPFP.Template template,
            DPFP.Verification.Verification.Result result
        ) {
            Stream stream;

            for (int index = 0; employees.Length > index; index++)
            {
                byte[] raw = Convert.FromBase64String(employees[index].status.Replace(" ", "+"));
                stream = new MemoryStream(raw);


                template = new DPFP.Template(stream);
                Verificator.Verify(features, template, ref result);
                UpdateStatus(result.FARAchieved);

                if (result.Verified)
                {
                    Console.WriteLine("Matched! isSomeoneVerified: true");
                    this.verifiedEmployee = employees[index];
                    this.isSomeoneVerified = true;
                    break;
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
            SetPrompt(dal.SaveAttendance(employee.id, _datetime));
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
