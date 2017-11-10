using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public static class Auth
    {
        private const string Domain = "hip.eu.auth0.com";
        private const string SubClaimType = "https://hip.cs.upb.de/sub";
        private const string RolesClaimType = "https://hip.cs.upb.de/roles";
        private const string Auth0ManagementApiAudience = "https://hip.eu.auth0.com/api/v2/";

        /// <summary>
        /// Gets the Auth0 user ID.
        /// </summary>
        public static string GetUserIdentity(this IIdentity identity)
        {
            return (identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == SubClaimType)?
                .Value;
        }

        /// <summary>
        /// Gets the roles of the current user.
        /// </summary>
        public static IReadOnlyList<Claim> GetUserRoles(this IIdentity identity)
        {
            return (identity as ClaimsIdentity)?.FindAll(c => c.Type == RolesClaimType).ToList() ?? new List<Claim>();
        }

        /// <summary>
        /// Gets the roles of the specified user by querying the Auth0 Management API.
        /// </summary>
        public static async Task<IReadOnlyList<Claim>> GetUserRolesAsync(string userId, AuthConfig authConfig)
        {
            var roles = await GetUserRolesAsStringAsync(userId, authConfig);
            return roles.Select(role => new Claim(RolesClaimType, role)).ToList();
        }

        /// <summary>
        /// Gets the roles of the specified user by querying the Auth0 Management API.
        /// </summary>
        public static async Task<IReadOnlyList<string>> GetUserRolesAsStringAsync(string userId, AuthConfig authConfig)
        {
            // Note: Reading 'user.AppMetadata' requires the scope 'read:users_app_metadata'
            var accessToken = await GetAccessTokenAsync(authConfig);
            var management = new ManagementApiClient(accessToken, Domain);
            var user = await management.Users.GetAsync(userId);
            var roles = (JArray)user.AppMetadata.roles;
            return roles.Select(role => role.ToString()).ToList();
        }

        /// <summary>
        /// Gets a list of all users, their IDs and their roles.
        /// </summary>
        /// <param name="authConfig"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetUsersWithRolesAsync(AuthConfig authConfig)
        {
            var accessToken = await GetAccessTokenAsync(authConfig);
            var management = new ManagementApiClient(accessToken, Domain);
            var users = await management.Users.GetAllAsync(fields: "user_id,app_metadata.roles");

            return users.ToDictionary(
                u => u.UserId,
                u => ((JArray)u.AppMetadata.roles).Select(role => role.ToString()).ToList() as IReadOnlyList<string>);
        }

        public static async Task SetUserRolesAsync(string userId, IEnumerable<string> roles, AuthConfig authConfig)
        {
            var accessToken = await GetAccessTokenAsync(authConfig);
            var patch = new { app_metadata = new { roles } };
            
            // Apparently, Auth0 Management API client does not support updating app_metadata of a user,
            // so we use an HttpClient and do that manually -.-
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://{Domain}/api/v2/users/{userId}"),
                Method = new HttpMethod("PATCH"),
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                Content = new StringContent(JsonConvert.SerializeObject(patch), Encoding.UTF8, "application/json")
            };

            var http = new HttpClient();
            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode(); // TODO: 400 Bad Request
        }
        
        /// <summary>
        /// Requests an access token for the microservice that can be used to access the Auth0 Management API.
        /// </summary>
        private static async Task<string> GetAccessTokenAsync(AuthConfig authConfig)
        {
            // TODO optimization: In the future we should cache the resulting access token (since it's valid for 24 hours)

            var client = new AuthenticationApiClient(Domain);

            var response = await client.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                Audience = Auth0ManagementApiAudience,
                ClientId = authConfig.ClientId,
                ClientSecret = authConfig.ClientSecret
            });

            return response.AccessToken;
        }
    }
}
