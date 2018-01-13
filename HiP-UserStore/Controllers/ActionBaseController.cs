using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using System;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using ResourceType = PaderbornUniversity.SILab.Hip.UserStore.Model.ResourceType;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    /// <summary>
    /// Base class for creating controllers for a specific action type
    /// </summary>
    /// <typeparam name="TArgs">Type of arguments</typeparam>
    [Authorize]
    [Route("api/Actions/[controller]")]
    public abstract class ActionBaseController<TArgs> : BaseController<TArgs> where TArgs : ActionArgs
    {
        // ReSharper disable All
        protected readonly EntityIndex _entityIndex;
        protected readonly EventStoreService _eventStore;
        // ReSharper Restore All


        public ActionBaseController(EventStoreService eventStore, InMemoryCache cache)
        {
            _eventStore = eventStore;
            _entityIndex = cache.Index<EntityIndex>();
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Post([FromBody] TArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = await ValidateActionArgs(args);
            if (!validationResult.Success)
                return validationResult.ActionResult;

            var ev = new ActionCreated
            {
                Id = _entityIndex.NextId(ResourceType.Action),
                UserId = User.Identity.GetUserIdentity(),
                Properties = args,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            return Created($"{Request.Scheme}://{Request.Host}/api/Action/{ev.Id}", ev.Id);
        }
    }
}

