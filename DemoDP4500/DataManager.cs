using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Enrollment
{
    public class DataManager
    {
        // ApiHandler Test
        protected ApiHandler Api;

        // Http Client
        private HttpClient client = new HttpClient();
        private List<PendingAttendance> pendingAttendances = new List<PendingAttendance>{ };

        EmployeeModel[] employees = new EmployeeModel[0];

        public DataManager()
        {
            this.Api = new ApiHandler();
        }

        #region Api Calls
        public void getConnection()
        {
            var response = this.client.GetAsync("https://dev.timetracker.chronostep.com/");
            response.Wait();
            var result = response.Result;
        }

        /**
         * Get all employees
         * param: Integer is_all
         * param: String search
         * 
         * return: 
         */
        public EmployeeModel[] getEmployees(int is_all, string search = null)
        {
            KeyValuePair<string, bool> response = this.Api.apiRequest(HttpMethod.Get, "employees?all=" + is_all + "&search=" + search);
            return response.Value ? JsonConvert.DeserializeObject<EmployeeModel[]>(response.Key) : new EmployeeModel[0];
        }

        /*
         * Get attendance with in days
         */
        public AttendanceModel[] getLatestAttendances()
        {
            KeyValuePair<string, bool> response = this.Api.apiRequest(HttpMethod.Get, "attendances");
            return response.Value ? JsonConvert.DeserializeObject<AttendanceModel[]>(response.Key) : new AttendanceModel[0];
        }

        /*
         * Register new fingerprint
         */
        public void updateRecordAsync(string fingerprint, int employee_id)
        {
            var payload = new Dictionary<string, string>
            {
                { "fingerprint", fingerprint},
                { "employee_id", employee_id.ToString()}
            };
            StringContent content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            var response = this.Api.apiRequest(HttpMethod.Post, "employee/update", content);

            // Retrieve attendance data after submit
            FileHandler.saveData(new AttendanceModel());

            // Retrieve employee data after submit
            FileHandler.saveData(new EmployeeModel());

            content.Dispose();
        }

        /*
         * Save employees by batch 
         */
        public bool SaveBatchAttendance(int id, DateTime dateTime)
        {
            var payload = new Dictionary<string, string>
            {
                { "empID", id.ToString() },
                { "dateTime", dateTime.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string strContent = JsonConvert.SerializeObject(payload);
            StringContent content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = this.Api.apiRequest(HttpMethod.Post, "attendance", content);

            return response.Value;
        }

        /*
         * Save attendance into the live server
         */
        public string SaveAttendance(int id, DateTime dateTime)
        {
            Console.WriteLine(id);
            string _return = "";
            var payload = new Dictionary<string, string>
            {
                { "empID", id.ToString() },
                { "dateTime", dateTime.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            try
            {
                string strContent = JsonConvert.SerializeObject(payload);
                StringContent content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
                var response = this.Api.apiRequest(HttpMethod.Post, "attendance", content);

                if (response.Value)
                {
                    string resBody = response.Key;

                    if (resBody.Contains("Insert"))
                    {
                        _return = "Successfully Time in";
                    }
                    else if (resBody.Contains("Updated"))
                    {
                        _return = "Successfully Time out";
                    }
                    else
                    {
                        _return = "Too early to " + (resBody.Contains("Time_in") ? "Time in!" : "Time out!");
                    }
                }
                else
                {
                    PendingAttendance att = new PendingAttendance();
                    pendingAttendances = new List<PendingAttendance>(FileHandler.retrieveData(new PendingAttendance(), "pending"));

                    att.emp_id = id;
                    att.date = dateTime;
                    pendingAttendances.Add(att);

                    string serializedData = JsonConvert.SerializeObject(pendingAttendances, Formatting.Indented);
                    FileHandler.FingerPrintLogger(serializedData, att.getFileName(), "pendings", false);
                }
            }
            catch (Exception e)
            {
                PendingAttendance att = new PendingAttendance();
                pendingAttendances = new List<PendingAttendance>(FileHandler.retrieveData(new PendingAttendance(), "pending"));

                att.emp_id = id;
                att.date = dateTime;
                pendingAttendances.Add(att);

                string serializedData = JsonConvert.SerializeObject(pendingAttendances, Formatting.Indented);
                FileHandler.FingerPrintLogger(serializedData, att.getFileName(), "pendings", false);
            }


            Task.Factory.StartNew(() => {
                // Retrieve attendance data after submit
                FileHandler.saveData(new AttendanceModel());

                // Retrieve employee data after submit
                FileHandler.saveData(new EmployeeModel());
            });

            return _return;
        }
        #endregion

        #region Log Validation:
        /*
         * Prepare the response message after the time in | time out
         */
        public string FingerprintResponseMsg(int id, DateTime dateTime)
        {
            string _return = "";
            AttendanceModel[] attendances = FileHandler.retrieveData(new AttendanceModel());
            AttendanceModel attendance = Array.Find(attendances, att => (att != null)?att.employee_id == id: false);
            int attIndex = Array.IndexOf(attendances, attendance);

            if (attendance.time_in != null && attendance.time_out != null)
            {
                if (IsTooEarly(dateTime, attendance.time_out))
                {
                    _return = "Too early to time in!";
                } else
                {
                    _return = "Successfully Time in!";
                    attendance.time_in = dateTime.ToString();
                    attendance.time_out = null;
                    UpdateAttendanceFile(attendances, attendance, attIndex);
                }
            } else if (attendance.time_in != null && attendance.time_out == null)
            {
                if (IsTooEarly(dateTime, attendance.time_in))
                {
                    _return = "Too early to time out!";
                }
                else
                {
                    _return = "Successfully Time out!";
                    attendance.time_out = dateTime.ToString();
                    UpdateAttendanceFile(attendances, attendance, attIndex);
                }
            }
            return _return;
        }
        #endregion

        #region Helper Methods
        /*
         * Identifies the date of time in or time out is too early
         */
        public bool IsTooEarly(DateTime dateTime, string dateInOrOut)
        {
            int interval = 20;
            DateTime dateTimeInOut = Convert.ToDateTime(dateInOrOut);
            Console.WriteLine(String.Format("{0} {1}", dateTime, dateTimeInOut));
            // add 20 seconds for interval
            dateTimeInOut = dateTimeInOut.AddSeconds(interval);

            return DateTime.Compare(dateTimeInOut, dateTime) >= 0;
        }

        /*
         * Update the attendance file upon the changes in the attendance object
         */
        public void UpdateAttendanceFile(AttendanceModel[] attendances, AttendanceModel attendance, int index)
        {
            attendances[index] = attendance;
            string attendanceData = JsonConvert.SerializeObject(attendances, Formatting.Indented);
            FileHandler.writeFile(attendance.getFileName(), attendanceData);
        }
        #endregion
    }
}
