

using System.Net;
using System.Configuration;
using System.Security.Claims;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using System.Globalization;

/// A "reduced" user object containing only a few attributes

namespace FunctionAppAzureAD
{
    public class AADUser
    {
        public string displayName { get; set; }
        public string userPrincipalName { get; set; }
        public string mobilePhone { get; set; }
    }
    public static class FunctionAppAzureAD
    {
        [FunctionName("FunctionAppAzureAD")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)

        {
            log.Verbose($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            

            string resourceId = "https://shridemo.onmicrosoft.com/512ea907-63b5-413f-8ade-8c8e83d5d06d";
            string tenantId = "8bb85694-c435-4384-b6cd-3eabc7066bf7";
            string tenant = "shridemo.onmicrosoft.com";
            string authString = String.Empty;
            string upn = "simulateddevice@shridemo.onmicrosoft.com";
            string clientId = "97cc5195-c220-47b9-ab7c-5c082975eef2";
            string clientSecret = "GQ/F+bFwfm6zJZGiEXcOQ7/QNkOd/Wki2lSXVzmJs30=";
            string aadInstance = "https://login.microsoftonline.com/{0}";

            UserPasswordCredential userPasswordCredential = new UserPasswordCredential(upn, "Offsite@123");

            authString = "https://login.microsoftonline.com/8bb85694-c435-4384-b6cd-3eabc7066bf7/oauth2/token";
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            var authenticationContext = new AuthenticationContext(authority, false);
            //ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            //var result = authenticationContext.AcquireTokenSilentAsync(resourceId, clientId).Result;
           
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(resourceId, clientId, userPasswordCredential);
            string token = authenticationResult.AccessToken;
            log.Verbose(token);

            var outputName = String.Empty;
            var responseString = String.Empty;
            var phone = String.Empty;

            using (var client = new HttpClient())
            {
                string requestUrl = $"https://graph.microsoft.com/v1.0/users/{upn}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                log.Verbose(request.ToString());
                HttpResponseMessage response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<AADUser>(responseString);
                phone = user.mobilePhone;
                outputName = user.displayName;
                log.Verbose(responseString);
            }
            return req.CreateResponse(HttpStatusCode.OK, authenticationResult.AccessToken, JsonMediaTypeFormatter.DefaultMediaType);

        }

        
    }

    


}