using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class ApiContentClient
    {
        public async Task<string> GetResponseFromUrlAsString(string urlPath, string token)
        {
            var response = await GetHttpWebResponse(urlPath, token);
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                var responseString = reader.ReadToEnd();
                if (responseString.Equals(""))
                    return "unsuccessful";
                else
                    return responseString;
            }
        }

        private async Task<HttpWebResponse> GetHttpWebResponse(string urlPath, string token)
        {
            try
            {
                var request = WebRequest.Create(urlPath) as HttpWebRequest;
                if (request != null)
                {
                    request.Headers["Authorization"] = token;
                    request.Accept = "application/json";
                }
                var response = (HttpWebResponse)await request.GetResponseAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return response;
                    default:
                        throw new WebException($"Unexpected response status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                var webEx = ex as WebException;
                if (webEx.Response != null)
                    return (HttpWebResponse)webEx.Response;
                else
                    return new HttpWebResponse();
            }
        }
    }
}
