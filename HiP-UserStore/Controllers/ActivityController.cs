using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/Users")]
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly EndpointConfig _endpointConfig;

        public ActivityController(IOptions<EndpointConfig> endpointConfig)
        {
            _endpointConfig = endpointConfig.Value;
        }

        [HttpGet("Activity")]
        [ProducesResponseType(typeof(ActivityResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAllActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var activityResult = new ActivityResult
            {
                ExhibitIds = await GetContentIdsAsync(status, token, ResourceType.Exhibits.Name),
                RouteIds = await GetContentIdsAsync(status, token, ResourceType.Routes.Name),
                MediaIds = await GetContentIdsAsync(status, token, ResourceType.Media.Name),
                TagIds = await GetContentIdsAsync(status, token, ResourceType.Tags.Name),
                ExhibitPageIds = await GetContentIdsAsync(status, token, ResourceType.ExhibitPages.Name)
            };

            // remove Ids of content which does not contain changes
            activityResult.ExhibitIds = await RemoveIdsAsync(activityResult.ExhibitIds, ResourceType.Exhibits.Name,
                userId, token);
            activityResult.RouteIds = await RemoveIdsAsync(activityResult.RouteIds, ResourceType.Routes.Name,
                userId, token);
            activityResult.MediaIds = await RemoveIdsAsync(activityResult.MediaIds, ResourceType.Media.Name,
                userId, token);
            activityResult.TagIds = await RemoveIdsAsync(activityResult.TagIds, ResourceType.Tags.Name,
                userId, token);
            activityResult.ExhibitPageIds = await RemoveIdsAsync(activityResult.ExhibitPageIds, ResourceType.ExhibitPages.Name,
                userId, token);

            return Ok(activityResult);
        }

        [HttpGet("Activity/Exhibits")]
        [ProducesResponseType(typeof(IReadOnlyCollection<int>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetExhibitActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var exhibits = await GetContentIdsAsync(status, token, ResourceType.Exhibits.Name);
            exhibits = await RemoveIdsAsync(exhibits, ResourceType.Exhibits.Name,
                userId, token);

            return Ok(exhibits);
        }

        [HttpGet("Activity/Routes")]
        [ProducesResponseType(typeof(IReadOnlyCollection<int>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetRouteActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var routes = await GetContentIdsAsync(status, token, ResourceType.Routes.Name);
            routes = await RemoveIdsAsync(routes, ResourceType.Routes.Name,
                userId, token);

            return Ok(routes);
        }

        [HttpGet("Activity/Tags")]
        [ProducesResponseType(typeof(IReadOnlyCollection<int>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetTagActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var tags = await GetContentIdsAsync(status, token, ResourceType.Tags.Name);
            tags = await RemoveIdsAsync(tags, ResourceType.Tags.Name,
                userId, token);

            return Ok(tags);
        }

        [HttpGet("Activity/Media")]
        [ProducesResponseType(typeof(IReadOnlyCollection<int>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetMediaActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var media = await GetContentIdsAsync(status, token, ResourceType.Media.Name);
            media = await RemoveIdsAsync(media, ResourceType.Media.Name,
                userId, token);

            return Ok(media);
        }

        [HttpGet("Activity/Exhibits/Pages")]
        [ProducesResponseType(typeof(IReadOnlyCollection<int>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetExhibitPageActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();
            var token = Request.Headers["Authorization"];

            var exhibitPages = await GetContentIdsAsync(status, token, ResourceType.ExhibitPages.Name);
            exhibitPages = await RemoveIdsAsync(exhibitPages, ResourceType.ExhibitPages.Name,
                userId, token);

            return Ok(exhibitPages);
        }

        private async Task<List<int>> GetContentIdsAsync(ContentStatus status, string token, string type)
        {
            var ids = new List<int>();

            switch (type)
            {
                case "Exhibits":
                    var exhibitsClient = new ExhibitsClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var exhibits = await exhibitsClient.GetAsync(status: status);
                    foreach (var exhibit in exhibits.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibit.UserId))
                            ids.Add(exhibit.Id);
                    break;
                case "Routes":
                    var routesClient = new RoutesClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var routes = await routesClient.GetAsync(status: status);
                    foreach (var route in routes.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, route.UserId))
                            ids.Add(route.Id);
                    break;
                case "Tags":
                    var tagsClient = new TagsClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var tags = await tagsClient.GetAllAsync(status: status);
                    foreach (var tag in tags.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, tag.UserId))
                            ids.Add(tag.Id);
                    break;
                case "Media":
                    var mediaClient = new MediaClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var media = await mediaClient.GetAsync(status: status);
                    foreach (var medium in media.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, medium.UserId))
                            ids.Add(medium.Id);
                    break;
                case "ExhibitsPages":
                    var exhibitPagesClient = new ExhibitPagesClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var exhibitPages = await exhibitPagesClient.GetAllPagesAsync(status: status);
                    foreach (var exhibitPage in exhibitPages.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibitPage.UserId))
                            ids.Add(exhibitPage.Id);
                    break;
            }
            return ids;
        }

        private async Task<HistorySummary> GetHistorySummary(int id, string resourceType, string token)
        {
            var client = new HistoryClient(_endpointConfig.DataStoreUrl) { Authorization = token+2 };
            try
            {
                switch (resourceType)
                {
                    case "Exhibits":
                        var exhibitHistorySummary = await client.GetExhibitSummaryAsync(id: id);
                        return exhibitHistorySummary;
                    case "Routes":
                        var routeHistorySummary = await client.GetRouteSummaryAsync(id: id);
                        return routeHistorySummary;
                    case "Media":
                        var mediaHistorySummary = await client.GetMediaSummaryAsync(id: id);
                        return mediaHistorySummary;
                    case "Tags":
                        var tagHistorySummary = await client.GetTagSummaryAsync(id: id);
                        return tagHistorySummary;
                    case "ExhibitsPages":
                        var exhibitPageHistorySummary = await client.GetExhibitPageSummaryAsync(id: id);
                        return exhibitPageHistorySummary;
                    default:
                        return new HistorySummary();
                }
            } catch (SwaggerException)
            {
                return new HistorySummary();
            }
        }

        private async Task<List<int>> RemoveIdsAsync(List<int> ids, string resourceType, string userId, string token)
        {
            for (var i = ids.Count - 1; i >= 0; i--)
            {
                var summary = await GetHistorySummary(ids.ElementAt(i), resourceType, token);

                if (summary.Changes != null)
                {
                    if (summary.Changes.Count == 0)
                        ids.RemoveAt(i);
                    else if (summary.Changes.Last().UserId.Equals(userId) || summary.Changes.Last().Description.Equals("Created"))
                        ids.RemoveAt(i);
                }
                else
                {
                    ids.RemoveAt(i);
                }
                    
            }
            return ids;
        }
    }
}
