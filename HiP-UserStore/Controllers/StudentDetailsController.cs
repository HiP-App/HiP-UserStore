using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/Users")]
    [Authorize]
    public class StudentDetailsController : Controller
    {
        private readonly EventStoreClient _eventStore;
        private readonly UserIndex _userIndex;

        public StudentDetailsController(EventStoreClient eventStore, InMemoryCache cache)
        {
            _eventStore = eventStore;
            _userIndex = cache.Index<UserIndex>();
        }

        [HttpPut("{userId}/StudentDetails")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutAsync(string userId, StudentDetailsArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            var ev = new UserStudentDetailsUpdated
            {
                Id = internalId,
                UserId = User.Identity.GetUserIdentity(),
                Timestamp = DateTimeOffset.Now,
                Properties = args
            };

            await _eventStore.AppendEventAsync(ev);
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

            // Deletion is represented by an update-event with empty properties
            var ev = new UserStudentDetailsUpdated
            {
                Id = internalId,
                UserId = User.Identity.GetUserIdentity(),
                Timestamp = DateTimeOffset.Now,
            };

            await _eventStore.AppendEventAsync(ev);
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
