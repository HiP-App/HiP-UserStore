using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                ExhibitIds = await GetContentIdsAsync(status, token, ResourceTypes.Exhibit),
                RouteIds = await GetContentIdsAsync(status, token, ResourceTypes.Route),
                MediaIds = await GetContentIdsAsync(status, token, ResourceTypes.Media),
                TagIds = await GetContentIdsAsync(status, token, ResourceTypes.Tag),
                ExhibitPageIds = await GetContentIdsAsync(status, token, ResourceTypes.ExhibitPage)
            };

            // Remove ids of content which does not contain changes
            activityResult.ExhibitIds = await RemoveIdsAsync(activityResult.ExhibitIds, ResourceTypes.Exhibit,
                userId, token);
            activityResult.RouteIds = await RemoveIdsAsync(activityResult.RouteIds, ResourceTypes.Route,
                userId, token);
            activityResult.MediaIds = await RemoveIdsAsync(activityResult.MediaIds, ResourceTypes.Media,
                userId, token);
            activityResult.TagIds = await RemoveIdsAsync(activityResult.TagIds, ResourceTypes.Tag,
                userId, token);
            activityResult.ExhibitPageIds = await RemoveIdsAsync(activityResult.ExhibitPageIds, ResourceTypes.ExhibitPage,
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

            var exhibits = await GetContentIdsAsync(status, token, ResourceTypes.Exhibit);
            exhibits = await RemoveIdsAsync(exhibits, ResourceTypes.Exhibit,
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

            var routes = await GetContentIdsAsync(status, token, ResourceTypes.Route);
            routes = await RemoveIdsAsync(routes, ResourceTypes.Route,
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

            var tags = await GetContentIdsAsync(status, token, ResourceTypes.Tag);
            tags = await RemoveIdsAsync(tags, ResourceTypes.Tag,
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

            var media = await GetContentIdsAsync(status, token, ResourceTypes.Media);
            media = await RemoveIdsAsync(media, ResourceTypes.Media,
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

            var exhibitPages = await GetContentIdsAsync(status, token, ResourceTypes.ExhibitPage);
            exhibitPages = await RemoveIdsAsync(exhibitPages, ResourceTypes.ExhibitPage,
                userId, token);

            return Ok(exhibitPages);
        }

        private async Task<List<int>> GetContentIdsAsync(ContentStatus status, string token, ResourceType type)
        {
            var ids = new List<int>();

            switch (type)
            {
                case ResourceType _ when type == ResourceTypes.Exhibit:
                    var exhibitsClient = new ExhibitsClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var exhibits = await exhibitsClient.GetAsync(status: status);
                    foreach (var exhibit in exhibits.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibit.UserId))
                            ids.Add(exhibit.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Route:
                    var routesClient = new RoutesClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var routes = await routesClient.GetAsync(status: status);
                    foreach (var route in routes.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, route.UserId))
                            ids.Add(route.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Tag:
                    var tagsClient = new TagsClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var tags = await tagsClient.GetAllAsync(status: status);
                    foreach (var tag in tags.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, tag.UserId))
                            ids.Add(tag.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Media:
                    var mediaClient = new MediaClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var media = await mediaClient.GetAsync(status: status);
                    foreach (var medium in media.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, medium.UserId))
                            ids.Add(medium.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.ExhibitPage:
                    var exhibitPagesClient = new ExhibitPagesClient(_endpointConfig.DataStoreUrl) { Authorization = token };
                    var exhibitPages = await exhibitPagesClient.GetAllPagesAsync(status: status);
                    foreach (var exhibitPage in exhibitPages.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibitPage.UserId))
                            ids.Add(exhibitPage.Id);
                    break;
            }
            return ids;
        }

        private async Task<HistorySummary> GetHistorySummary(int id, ResourceType resourceType, string token)
        {
            var client = new HistoryClient(_endpointConfig.DataStoreUrl) { Authorization = token };
            try
            {
                switch (resourceType)
                {
                    case ResourceType _ when resourceType == ResourceTypes.Exhibit:
                        var exhibitHistorySummary = await client.GetExhibitSummaryAsync(id: id);
                        return exhibitHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Route:
                        var routeHistorySummary = await client.GetRouteSummaryAsync(id: id);
                        return routeHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Media:
                        var mediaHistorySummary = await client.GetMediaSummaryAsync(id: id);
                        return mediaHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Tag:
                        var tagHistorySummary = await client.GetTagSummaryAsync(id: id);
                        return tagHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.ExhibitPage:
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

        private async Task<List<int>> RemoveIdsAsync(List<int> ids, ResourceType resourceType, string userId, string token)
        {
            for (var i = ids.Count - 1; i >= 0; i--)
            {
                var summary = await GetHistorySummary(ids[i], resourceType, token);

                if (summary.Changes != null)
                {
                    if (summary.Changes.Count == 0)
                        ids.RemoveAt(i);
                    else if (summary.Changes.Last().UserId == userId || summary.Changes.Last().Description == "Created")
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
