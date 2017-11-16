namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class AuthConfig
    {
        /// <summary>
        /// Audience of all HiP APIs.
        /// </summary>
        public string Audience { get; set; }

        public string Authority { get; set; }

        // Additional info for Auth-class:

        /// <summary>
        /// Auth0 domain.
        /// Example:"hip.eu.auth0.com"
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The claim from which the Auth0 user ID can be obtained.
        /// Example: "https://hip.cs.upb.de/sub"
        /// </summary>
        public string SubClaimType { get; set; }

        /// <summary>
        /// The claim from which the Auth0 user roles can be obtained.
        /// Example: "https://hip.cs.upb.de/roles"
        /// </summary>
        public string RolesClaimType { get; set; }

        /// <summary>
        /// Audience for the Auth0 Management Audience (used to access Auth0 Management API).
        /// Example: "https://hip.eu.auth0.com/api/v2/"
        /// </summary>
        public string Auth0ManagementApiAudience { get; set; }

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
