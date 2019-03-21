using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AasClient
{
    class ADTokenHelper
    {
        private static string authUrl = "https://login.windows.net";

        public async static Task<string> GetAccessToken(string domain, string resourceUrl, string clientId, string clientSecret)
        {
            var authority = $"{authUrl}/{domain}/oauth2/token";
            var authContext = new AuthenticationContext(authority);
            // Config for OAuth client credentials 
            var clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authenticationResult = await authContext.AcquireTokenAsync(resourceUrl, clientCred);
            //get access token
            return authenticationResult.AccessToken;
        }
    }
}
