using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
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
    /// <remarks>
    /// This class contains extension methods and as such is static and not registered as a
    /// service for dependency injection.
    /// </remarks>
    public static class Auth
    {
        private static UserStoreAuthConfig _authConfig;

        public static void Initialize(IOptions<UserStoreAuthConfig> authConfig)
        {
            _authConfig = authConfig.Value;
        }

        /// <summary>
        /// Gets the Auth0 user ID.
        /// </summary>
        public static string GetUserIdentity(this IIdentity identity)
        {
            return (identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == _authConfig.SubClaimType)?
                .Value;
        }

        /// <summary>
        /// Gets the roles of the current user.
        /// </summary>
        public static IReadOnlyList<Claim> GetUserRoles(this IIdentity identity)
        {
            return (identity as ClaimsIdentity)?.FindAll(c => c.Type == _authConfig.RolesClaimType).ToList() ?? new List<Claim>();
        }

        /// <summary>
        /// Gets the roles of the specified user by querying the Auth0 Management API.
        /// </summary>
        public static async Task<IReadOnlyList<Claim>> GetUserRolesAsync(string userId)
        {
            var roles = await GetUserRolesAsStringAsync(userId);
            return roles.Select(role => new Claim(_authConfig.RolesClaimType, role)).ToList();
        }

        /// <summary>
        /// Gets the roles of the specified user by querying the Auth0 Management API.
        /// </summary>
        public static async Task<IReadOnlyList<string>> GetUserRolesAsStringAsync(string userId)
        {
            // Note: Reading 'user.AppMetadata' requires the scope 'read:users_app_metadata'
            var accessToken = await GetAccessTokenAsync();
            var management = new ManagementApiClient(accessToken, _authConfig.Domain);
            var user = await management.Users.GetAsync(userId);
            var roles = (JArray)user.AppMetadata?.roles ?? new JArray();
            return roles.Select(role => role.ToString()).ToList();
        }

        /// <summary>
        /// Gets a list of all users, their IDs and their roles.
        /// </summary>
        public static async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetUsersWithRolesAsync()
        {
            var accessToken = await GetAccessTokenAsync();
            var management = new ManagementApiClient(accessToken, _authConfig.Domain);
            var users = await management.Users.GetAllAsync(fields: "user_id,app_metadata.roles");

            return users.ToDictionary(
                u => u.UserId,
                u =>
                {
                    var rolesArray = (JArray)u.AppMetadata?.roles ?? new JArray();
                    return rolesArray.Select(role => role.ToString()).ToList() as IReadOnlyList<string>;
                });
        }

        /// <summary>
        /// Assigns the specified roles to the specified user.
        /// </summary>
        /// <exception cref="HttpRequestException">Auth0 reported an error during the process</exception>
        public static async Task SetUserRolesAsync(string userId, IEnumerable<string> roles)
        {
            var accessToken = await GetAccessTokenAsync();
            var patch = new { app_metadata = new { roles } };

            // Apparently, Auth0 Management API client does not support updating app_metadata of a user,
            // so we use an HttpClient and do that manually -.-
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://{_authConfig.Domain}/api/v2/users/{userId}"),
                Method = new HttpMethod("PATCH"),
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                Content = new StringContent(JsonConvert.SerializeObject(patch), Encoding.UTF8, "application/json")
            };

            var http = new HttpClient();
            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Registers a new user in Auth0 and assigns it the role 'Student'.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">Email or password is missing</exception>
        /// <exception cref="ApiException">Auth0 reported an error during registration</exception>
        public static async Task<string> CreateUserAsync(UserRegistrationArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (string.IsNullOrEmpty(args.Email))
                throw new ArgumentException("Email is required", nameof(args));

            if (string.IsNullOrEmpty(args.Password))
                throw new ArgumentException("Password is required", nameof(args));

            var accessToken = await GetAccessTokenAsync();
            var management = new ManagementApiClient(accessToken, _authConfig.Domain);
            var user = await management.Users.CreateAsync(new UserCreateRequest
            {
                Email = args.Email,
                FirstName = args.FirstName,
                LastName = args.LastName,
                FullName = string.Join(' ', args.FirstName?.Trim() ?? "", args.LastName?.Trim() ?? ""),
                Password = args.Password,
                AppMetadata = new { roles = new[] { UserRoles.Student.ToString() } },
                Connection = "Username-Password-Authentication" // TODO: Make configurable
            });            

            return user.UserId;
        }

        /// <summary>
        /// Requests an access token for the microservice that can be used to access the Auth0 Management API.
        /// </summary>
        private static async Task<string> GetAccessTokenAsync()
        {
            // TODO optimization: In the future we should cache the resulting access token (since it's valid for 24 hours)

            var client = new AuthenticationApiClient(_authConfig.Domain);

            var response = await client.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                Audience = _authConfig.Auth0ManagementApiAudience,
                ClientId = _authConfig.ClientId,
                ClientSecret = _authConfig.ClientSecret
            });

            return response.AccessToken;
        }
    }
}
