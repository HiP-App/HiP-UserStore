namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class EndpointConfig
    {
        /// <summary>
        /// Connection string for the Mongo DB cache database.
        /// Example: "mongodb://localhost:27017"
        /// </summary>
        public string MongoDbHost { get; set; }

        /// <summary>
        /// Name of the database to use.
        /// Example: "main"
        /// </summary>
        public string MongoDbName { get; set; }
        
        /// <summary>
        /// URL that points to the "swagger.json" file. If set, this URL is entered by default
        /// when accessing the Swagger UI page. If not set, we will try to construct the URL
        /// automatically which might result in an invalid URL.
        /// </summary>
        public string SwaggerEndpoint { get; set; }

        /// <summary>
        /// URL pattern for generating thumbnail URLs. Should contain a placeholder "{0}" that is replaced with the
        /// user ID of the queried profile photo at runtime. The endpoint should support GET and DELETE requests.
        /// Example:
        /// "https://docker-hip.cs.upb.de/develop/thumbnailservice/api/Thumbnails?Url=userstore/api/Photo/{0}/File"
        /// </summary>
        /// <remarks>
        /// This property is optional: If no value is provided, no thumbnail URLs are generated - instead, direct URLs
        /// to the original image files are then returned.
        /// </remarks>
        public string ThumbnailUrlPattern { get; set; }

        /// <summary>
        /// URL that points to the API of the Data Store. Should end with .../datastore/
        /// Example:
        /// "https://docker-hip.cs.uni-paderborn.de/develop/datastore/"
        /// </summary>
        public string DataStoreUrl { get; set; }
    }
}
