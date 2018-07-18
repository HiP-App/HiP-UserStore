using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/Notifications/[controller]")]
    [Authorize]
    public abstract class NotificationBaseController<TArgs> : Controller where TArgs : NotificationBaseArgs, new()
    {
        private EventStoreService _eventStore;
        private EntityIndex _entityIndex;

        /// <summary>
        /// This resource type is used for creating the events for the EventStore
        /// </summary>
        protected abstract ResourceType ResourceType { get; }

        public NotificationBaseController(EventStoreService eventStore, InMemoryCache cache)
        {
            _eventStore = eventStore;
            _entityIndex = cache.Index<EntityIndex>();
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateNotification([FromBody]TArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (User.Identity.GetUserIdentity() == null)
                return Forbid();

            //the BaseResourceType is used in this case since we want to have following id's even for different types
            var id = _entityIndex.NextId(ResourceTypes.Notification);
            await EntityManager.CreateEntityAsync(_eventStore, args, ResourceType, id, User.Identity.GetUserIdentity());
            return Created($"{Request.Scheme}://{Request.Host}/api/Notification/{id}", id);
        }       

    }
}
