using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly EventStoreClient _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly EntityIndex _entityIndex;
        private readonly UserIndex _userIndex;
        private readonly EndpointConfig _endpointConfig;
        private readonly AuthConfig _authConfig;

        public UsersController(EventStoreClient eventStore, CacheDatabaseManager db, InMemoryCache cache,
            IOptions<EndpointConfig> endpointConfig, IOptions<AuthConfig> authConfig)
        {
            _eventStore = eventStore;
            _db = db;
            _entityIndex = cache.Index<EntityIndex>();
            _userIndex = cache.Index<UserIndex>();
            _endpointConfig = endpointConfig.Value;
            _authConfig = authConfig.Value;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResult>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllAsync()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToGetAll(User.Identity))
                return Forbid();

            var users = _db.Database.GetCollection<User>(ResourceType.User.Name)
                .AsQueryable()
                .ToList()
                .Select(async user => new UserResult(user)
                {
                    Roles = await Auth.GetUserRolesAsStringAsync(user.UserId, _authConfig),
                    ProfilePicture = GenerateFileUrl(user.UserId)
                });

            return Ok(await Task.WhenAll(users));
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByIdAsync(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToGet(User.Identity, userId))
                return Forbid();

            var user = _db.Database.GetCollection<User>(ResourceType.User.Name)
                .AsQueryable()
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            var result = new UserResult(user)
            {
                Roles = await Auth.GetUserRolesAsStringAsync(user.UserId, _authConfig),
                ProfilePicture = GenerateFileUrl(userId)
            };

            return Ok(result);
        }

        [HttpGet("Me")]
        [ProducesResponseType(typeof(UserResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSelfAsync() => await GetByIdAsync(User.Identity.GetUserIdentity());

        [HttpPut("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutAsync(string userId, [FromBody]UserArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            var ev = new UserUpdated
            {
                Id = internalId,
                UserId = User.Identity.GetUserIdentity(), // Note: refers to the API caller, not the updated user
                Properties = args,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            return NoContent();
        }

        [HttpPost("Invite")]
        public IActionResult InviteUsers(/*InviteArgs args*/)
        {
            throw new NotImplementedException(); // TODO: Migrate from CmsWebApi
        }

        /// <summary>
        /// This method is intended to be called by Auth0.
        /// A hook should be configured in Auth0 that calls this API after a user signed up.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)] // User with same ID already exists
        [ProducesResponseType(201)]
        public async Task<IActionResult> RegisterAsync([FromBody]UserArgsAuth0 args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserIdentity();

            if (_userIndex.Exists(userId))
                return StatusCode(409, new { Message = $"A user with ID '{userId}' already exists" });

            var ev = new UserCreated
            {
                Id = _entityIndex.NextId(ResourceType.User),
                UserId = userId,
                Timestamp = DateTimeOffset.Now,
                Properties = new UserArgs
                {
                    Email = args.Email,
                }
            };

            await _eventStore.AppendEventAsync(ev);
            return Created($"{Request.Scheme}://{Request.Host}/api/User/{userId}", userId);
        }

        private string GenerateFileUrl(string userId)
        {
            if (!string.IsNullOrWhiteSpace(_endpointConfig.ThumbnailUrlPattern))
            {
                // Generate thumbnail URL (if a thumbnail URL pattern is configured)
                return string.Format(_endpointConfig.ThumbnailUrlPattern, userId);
            }
            else
            {
                // Return direct URL
                return $"{Request.Scheme}://{Request.Host}/api/User/{userId}/Photo";
            }
        }
    }
}
