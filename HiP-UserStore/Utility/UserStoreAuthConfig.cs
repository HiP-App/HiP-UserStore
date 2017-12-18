using PaderbornUniversity.SILab.Hip.Webservice;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class UserStoreAuthConfig : AuthConfig
    {
        /// <summary>
        /// Auth0 domain.
        /// Default value: "hip.eu.auth0.com"
        /// </summary>
        public string Domain { get; set; } = "hip.eu.auth0.com";

        /// <summary>
        /// The claim from which the Auth0 user ID can be obtained.
        /// Default value: "https://hip.cs.upb.de/sub"
        /// </summary>
        public string SubClaimType { get; set; } = "https://hip.cs.upb.de/sub";

        /// <summary>
        /// The claim from which the Auth0 user roles can be obtained.
        /// Default value: "https://hip.cs.upb.de/roles"
        /// </summary>
        public string RolesClaimType { get; set; } = "https://hip.cs.upb.de/roles";

        /// <summary>
        /// Audience for the Auth0 Management Audience (used to access Auth0 Management API).
        /// Default value: "https://hip.eu.auth0.com/api/v2/"
        /// </summary>
        public string Auth0ManagementApiAudience { get; set; } = "https://hip.eu.auth0.com/api/v2/";

        /// <summary>
        /// ID of the non-interactive UserStore client (used to access Auth0 Management API).
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Secret of the non-interactive UserStore client (used to access Auth0 Management API).
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
