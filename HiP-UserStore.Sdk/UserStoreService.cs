using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PaderbornUniversity.SILab.Hip.UserStore
{
    /// <summary>
    /// A service that can be used with ASP.NET Core dependency injection.
    /// Usage: In ConfigureServices():
    /// <code>
    /// services.Configure&lt;UserStoreConfig&gt;(Configuration.GetSection("Endpoints"));
    /// services.AddSingleton&lt;UserStoreService&gt;();
    /// </code>
    /// </summary>
    public class UserStoreService
    {
        private readonly UserStoreConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserStoreService(IOptions<UserStoreConfig> config, ILogger<UserStoreService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config.Value;
            _httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrWhiteSpace(config.Value.UserStoreHost))
                logger.LogWarning($"{nameof(UserStoreConfig.UserStoreHost)} is not configured correctly!");
        }

        public ActivityClient Activity => new ActivityClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public PhotoClient Photo => new PhotoClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public StudentDetailsClient StudentDetails => new StudentDetailsClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public UsersClient Users => new UsersClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public ActionsClient Actions => new ActionsClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public ExhibitVisitedClient ExhibitVisitedAction => new ExhibitVisitedClient(_config.UserStoreHost)
        {
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };
    }
}
