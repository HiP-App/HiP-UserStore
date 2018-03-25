using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/Users")]
    [Authorize]
    public class StudentDetailsController : Controller
    {
        private readonly EventStoreService _eventStore;
        private readonly UserIndex _userIndex;

        public StudentDetailsController(EventStoreService eventStore, InMemoryCache cache)
        {
            _eventStore = eventStore;
            _userIndex = cache.Index<UserIndex>();
        }

        [HttpPut("{userId}/StudentDetails")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutAsync(string userId, [FromBody]StudentDetailsArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            var oldUser = await _eventStore.EventStream.GetCurrentEntityAsync<UserEventArgs>(ResourceTypes.User, internalId);

            var newUser = new UserEventArgs(oldUser)
            {
                StudentDetails = new StudentDetails(args)
            };

            await EntityManager.UpdateEntityAsync(_eventStore, oldUser, newUser, ResourceTypes.User,
                internalId, User.Identity.GetUserIdentity());

            return NoContent();
        }

        [HttpDelete("{userId}/StudentDetails")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAsync(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            var oldUser = await _eventStore.EventStream.GetCurrentEntityAsync<UserEventArgs>(ResourceTypes.User, internalId);

            //we need to set the StudentDetails to null to delete it
            var newUserArgs = new UserEventArgs(oldUser)
            {
                StudentDetails = null
            };

            await EntityManager.UpdateEntityAsync(_eventStore, oldUser, newUserArgs, ResourceTypes.User,
                internalId, User.Identity.GetUserIdentity());
            return NoContent();
        }

        [HttpGet("/api/Students/Disciplines")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public IActionResult GetDisciplines()
        {
            return Ok(new[]
            {
                "History",
                "Computer Science",
                "Medieval Studies",
                "History and Arts",
                "Arts",
                "Linguistics"
            }
            .OrderBy(s => s));
        }
    }
}
