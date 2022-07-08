using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;

namespace Enrollement
{
    public class ApiHandler
    {
        protected HttpClient client;
        protected string path;
        protected HttpRequestMessage request;

        public ApiHandler()
        {
            this.client = new HttpClient();
            this.request = new HttpRequestMessage();

            // Http client config
            this.path = (new Config()).ApiUrl;
            this.client.MaxResponseContentBufferSize = 2000000;
            this.request.Headers.Add("Accept", "application/json");
            this.request.Headers.Add("Authorization", "Bearer " + (new Config()).ApiAccesssToken);
        }

        /**
         * Api request handler 
         * param HttpMethod     method  Http request method
         * param string         url     Url request
         * 
         * ##### Optional Parameters #####
         * param StringContent  content Request body
         * param KeyValuePair   header
         * 
         * raturn string        response
         */
        public KeyValuePair<string, bool> apiRequest(HttpMethod method, string url, StringContent content = null, KeyValuePair<string, string> headers = new KeyValuePair<string, string>())
        {
            KeyValuePair<string, bool> returnVal = new KeyValuePair<string, bool>();
            this.request.Method     = method;
            this.request.Content    = content;
            this.request.RequestUri = new Uri(this.path + url);
            var response = this.client.SendAsync(this.request);
            
            try
            {
                response.Wait();

                var result = response.Result;
                var body = result.Content.ReadAsStringAsync();
                body.Wait();

                returnVal = new KeyValuePair<string, bool>(body.Result, result.IsSuccessStatusCode);
                
            } catch (Exception ex)
            {
                returnVal = new KeyValuePair<string, bool>(ex.Message, false);
            }

            return returnVal;
        }
    }
}
