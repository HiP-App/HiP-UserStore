namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class AuthConfig
    {
        /// <summary>
        /// Audience of all HiP APIs.
        /// </summary>
        public string Audience { get; set; }

        public string Authority { get; set; }

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
