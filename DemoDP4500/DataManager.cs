using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Enrollement
{

    public class EmployeeModel
    {
        public int id { get; set; }
        public int emp_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public int user_status { get; set; }
        public string status { get; set; }
        public object user_roles { get; set; }
        public object emp_statuses { get; set; }
        public string user_statuses { get; set; }
        public string full_name { get; set; }
    }

    public class AttendanceModel
    {
        public int id { get; set; }
        public int employee_id { get; set; }
        public string time_in { get; set; }
        public string time_out { get; set; }
    }

    public class DataManager
    {
        // Http Client
        private HttpClient client = new HttpClient();
        private static string path = (new Config()).ApiUrl;

        EmployeeModel[] employees = new EmployeeModel[0];

        public DataManager()
        {
            // patial assigning of uri
            this.client.MaxResponseContentBufferSize = 2000000;
        }

        public EmployeeModel[] getEmployee(int is_all, string search = null)
        {
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = this.client.GetAsync(path+"employees?all="+is_all+"&search="+search);
            try
            {
                response.Wait();
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var result = response.Result;
            if (result.IsSuccessStatusCode)
            {
                var body = result.Content.ReadAsAsync<EmployeeModel[]>();
                body.Wait();

                employees = body.Result;
            }

            return employees;
        }


        public AttendanceModel[] getAttendancesWithinNDays()
        {
            AttendanceModel[] attendances = { };
            var response = this.client.GetAsync(path + "attendances/3");
            try
            {
                response.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var result = response.Result;
            if (result.IsSuccessStatusCode)
            {
                var body = result.Content.ReadAsAsync<AttendanceModel[]>();
                body.Wait();

                attendances = body.Result;
            }

            return attendances;
        }

        public EmployeeModel[] getEmployeeByFingerprint(string fingerprint)
        {
            var payload = new Dictionary<string, string> {
                { "fingerprint", fingerprint }
            };
            
            var stringPayload = JsonConvert.SerializeObject(payload);
            StringContent content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path + "employee/fingerprint"),
                Content = content
            };
            var response = this.client.SendAsync(request);
            response.Wait();

            var result = response.Result;
            if (result.IsSuccessStatusCode)
            {
                var body = result.Content.ReadAsAsync<EmployeeModel[]>();
                body.Wait();

                employees = body.Result;
            }
            return employees;
        }

        public void updateRecordAsync(string fingerprint, int employee_id)
        {
            var payload = new Dictionary<string, string>
            {
                { "fingerprint", fingerprint},
                { "employee_id", employee_id.ToString()}
            };
            string strContent = JsonConvert.SerializeObject(payload);
            StringContent content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = this.client.PostAsync(path + "employee/update", content);
            response.Wait();

            var result = response.Result;
            if (result.IsSuccessStatusCode)
            {
                var body = result.Content.ReadAsStringAsync();
                body.Wait();
            }
            content.Dispose();
        }

        #region Log Validation:
        public string SaveAttendance(int id, DateTime dateTime)
        {
            string _return = "";
            var payload = new Dictionary<string, string>
            {
                { "empID", id.ToString() },
                { "dateTime", dateTime.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            string strContent = JsonConvert.SerializeObject(payload);
            StringContent content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = this.client.PostAsync(path + "attendance", content);
            response.Wait();

            var result = response.Result;
            if (result.IsSuccessStatusCode)
            {
                var body = result.Content.ReadAsStringAsync();
                body.Wait();

                string resBody = body.Result;
                
                if (resBody.Contains("Insert"))
                {
                    _return = "Successfully Time in";
                } else if (resBody.Contains("Updated"))
                {
                    _return = "Successfully Time out";
                }else
                {
                    _return = "Too early to " + (resBody.Contains("Time_in") ? "Time_in!": "Time_out!");
                }
            }

            return _return;
        }
        #endregion
    }
}
