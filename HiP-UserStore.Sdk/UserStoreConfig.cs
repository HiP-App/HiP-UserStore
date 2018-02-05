namespace PaderbornUniversity.SILab.Hip.UserStore
{
    /// <summary>
    /// Configuration properties for clients using the UserStore SDK.
    /// </summary>
    public sealed class UserStoreConfig
    {
        /// <summary>
        /// URL pointing to a running instance of the UserStore service.
        /// Example: "https://docker-hip.cs.upb.de/develop/userstore"
        /// </summary>
        public string UserStoreHost { get; set; }
    }
}
