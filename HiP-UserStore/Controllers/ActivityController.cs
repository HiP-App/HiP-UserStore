using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/Users")]
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly EndpointConfig _endpointConfig;
        private readonly DataStoreService _dataStoreService;
        private readonly ActivityIndex _activityIndex;

        public ActivityController(IOptions<EndpointConfig> endpointConfig, DataStoreService dataStoreService, InMemoryCache cache)
        {
            _endpointConfig = endpointConfig.Value;
            _dataStoreService = dataStoreService;
            _activityIndex = cache.Index<ActivityIndex>();
        }

        [HttpGet("Activity")]
        [ProducesResponseType(typeof(ActivityResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAllActivityAsync(ContentStatus status = ContentStatus.Published)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();

            var activityResult = new ActivityResult
            {
                ExhibitIds = await GetContentIdsAsync(status, ResourceTypes.Exhibit),
                RouteIds = await GetContentIdsAsync(status, ResourceTypes.Route),
                MediaIds = await GetContentIdsAsync(status, ResourceTypes.Media),
                TagIds = await GetContentIdsAsync(status, ResourceTypes.Tag),
                ExhibitPageIds = await GetContentIdsAsync(status, ResourceTypes.ExhibitPage)
            };

            // Remove ids of content which does not contain changes
            activityResult.ExhibitIds = await RemoveIdsAsync(activityResult.ExhibitIds, ResourceTypes.Exhibit, 
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Exhibit));
            activityResult.RouteIds = await RemoveIdsAsync(activityResult.RouteIds, ResourceTypes.Route,
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Route));
            activityResult.MediaIds = await RemoveIdsAsync(activityResult.MediaIds, ResourceTypes.Media,
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Media));
            activityResult.TagIds = await RemoveIdsAsync(activityResult.TagIds, ResourceTypes.Tag,
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Tag));
            activityResult.ExhibitPageIds = await RemoveIdsAsync(activityResult.ExhibitPageIds, ResourceTypes.ExhibitPage,
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.ExhibitPage));

            _activityIndex.UpdateAllTimestamps(userId, DateTimeOffset.Now);

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

            var exhibits = await GetContentIdsAsync(status, ResourceTypes.Exhibit);
            exhibits = await RemoveIdsAsync(exhibits, ResourceTypes.Exhibit, userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Exhibit));

            _activityIndex.UpdateTimestamp(userId, DateTimeOffset.Now, ResourceTypes.Exhibit);

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

            var routes = await GetContentIdsAsync(status, ResourceTypes.Route);
            routes = await RemoveIdsAsync(routes, ResourceTypes.Route, userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Route));

            _activityIndex.UpdateTimestamp(userId, DateTimeOffset.Now, ResourceTypes.Route);

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

            var tags = await GetContentIdsAsync(status, ResourceTypes.Tag);
            tags = await RemoveIdsAsync(tags, ResourceTypes.Tag, userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Tag));

            _activityIndex.UpdateTimestamp(userId, DateTimeOffset.Now, ResourceTypes.Tag);

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

            var media = await GetContentIdsAsync(status, ResourceTypes.Media);
            media = await RemoveIdsAsync(media, ResourceTypes.Media, userId, _activityIndex.GetTimestamp(userId, ResourceTypes.Media));

            _activityIndex.UpdateTimestamp(userId, DateTimeOffset.Now, ResourceTypes.Media);

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

            var exhibitPages = await GetContentIdsAsync(status, ResourceTypes.ExhibitPage);
            exhibitPages = await RemoveIdsAsync(exhibitPages, ResourceTypes.ExhibitPage, 
                userId, _activityIndex.GetTimestamp(userId, ResourceTypes.ExhibitPage));

            _activityIndex.UpdateTimestamp(userId, DateTimeOffset.Now, ResourceTypes.ExhibitPage);

            return Ok(exhibitPages);
        }

        private async Task<List<int>> GetContentIdsAsync(ContentStatus status, ResourceType type)
        {
            var ids = new List<int>();

            switch (type)
            {
                case ResourceType _ when type == ResourceTypes.Exhibit:
                    var exhibits = await _dataStoreService.Exhibits.GetAsync(status: status);
                    foreach (var exhibit in exhibits.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibit.UserId))
                            ids.Add(exhibit.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Route:
                    var routes = await _dataStoreService.Routes.GetAsync(status: status);
                    foreach (var route in routes.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, route.UserId))
                            ids.Add(route.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Tag:
                    var tags = await _dataStoreService.Tags.GetAllAsync(status: status);
                    foreach (var tag in tags.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, tag.UserId))
                            ids.Add(tag.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.Media:
                    var media = await _dataStoreService.Media.GetAsync(status: status);
                    foreach (var medium in media.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, medium.UserId))
                            ids.Add(medium.Id);
                    break;
                case ResourceType _ when type == ResourceTypes.ExhibitPage:
                    var exhibitPages = await _dataStoreService.ExhibitPages.GetAllPagesAsync(status: status);
                    foreach (var exhibitPage in exhibitPages.Items)
                        if (UserPermissions.IsAllowedToGetHistory(User.Identity, exhibitPage.UserId))
                            ids.Add(exhibitPage.Id);
                    break;
            }
            return ids;
        }

        private async Task<HistorySummary> GetHistorySummary(int id, ResourceType resourceType)
        {
            try
            {
                switch (resourceType)
                {
                    case ResourceType _ when resourceType == ResourceTypes.Exhibit:
                        var exhibitHistorySummary = await _dataStoreService.History.GetExhibitSummaryAsync(id: id);
                        return exhibitHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Route:
                        var routeHistorySummary = await _dataStoreService.History.GetRouteSummaryAsync(id: id);
                        return routeHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Media:
                        var mediaHistorySummary = await _dataStoreService.History.GetMediaSummaryAsync(id: id);
                        return mediaHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.Tag:
                        var tagHistorySummary = await _dataStoreService.History.GetTagSummaryAsync(id: id);
                        return tagHistorySummary;
                    case ResourceType _ when resourceType == ResourceTypes.ExhibitPage:
                        var exhibitPageHistorySummary = await _dataStoreService.History.GetExhibitPageSummaryAsync(id: id);
                        return exhibitPageHistorySummary;
                    default:
                        return new HistorySummary();
                }
            } catch (SwaggerException)
            {
                return new HistorySummary();
            }
        }

        private async Task<List<int>> RemoveIdsAsync(List<int> ids, ResourceType resourceType, string userId, DateTimeOffset time)
        {
            var finalIds = new List<int>(ids);

            for (var i = ids.Count - 1; i >= 0; i--)
            {
                var summary = await GetHistorySummary(ids[i], resourceType);
                var hasChanges = false;

                foreach (var change in summary.Changes)
                {
                    if (change.Timestamp < time) continue;

                    if (change.UserId != userId)
                    {
                        hasChanges = true;
                        break;
                    }
                }

                if (!hasChanges)
                    finalIds.RemoveAt(i);
            }
            return finalIds;
        }
    }
}
