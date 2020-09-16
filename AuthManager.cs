using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegrationPlaybook.LogicApps.ErrorReport
{
    public class AuthManager
    {
        

        /// <summary>
        /// Pass the SPN and azure subscription details and generate a OAUTH2.0 token
        /// </summary>
        /// <returns></returns>
        public ServiceClientCredentials GetAzureCredentials(string tenantId, string clientId, string clientSecret)
        {
            var authContext = new AuthenticationContext($"https://login.windows.net/{tenantId}");
            var credential = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authResult = authContext.AcquireTokenAsync("https://management.core.windows.net/", credential).Result;
            string token = authResult.CreateAuthorizationHeader().Substring("Bearer ".Length);

            return new TokenCredentials(token);
        }

     
    }
}
