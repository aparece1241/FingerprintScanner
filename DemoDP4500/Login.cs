using System;
using System.Text;
using System.Linq;
using Enrollement;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Enrollement
{

    public class LoginReponseModel
    {
        LoginReponseModel()
        {
        }
        public bool success { get; set; }
        public string message { get; set; }
        public string token { get; set; }
    }

    public class Login
    {
        protected static ApiHandler Api;
        protected static StringContent body;
        public Login()
        {
        }

        public static KeyValuePair<string, bool> Authenticate(string username, string password)
        {
            Api = new ApiHandler();

            var payload = new Dictionary<string, string> {
                {"employee_id", username },
                {"password", password }
            };

            string content = JsonConvert.SerializeObject(payload);
            body = new StringContent(content, Encoding.UTF8, "application/json");

            var response = Api.apiRequest(HttpMethod.Post, "device/login", body);
            var responseData = JsonConvert.DeserializeObject<LoginReponseModel>(response.Key);

            if (responseData.success)
            {
                frmMain.SetSetting("ApiAccessToken", responseData.token);
            }

            return new KeyValuePair<string, bool>(responseData.token, responseData.success);
        }
    }
}
