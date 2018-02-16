using Auth0.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly EventStoreService _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly EntityIndex _entityIndex;
        private readonly UserIndex _userIndex;
        private readonly EndpointConfig _endpointConfig;
        private readonly ILogger<UsersController> _logger;

        public UsersController(EventStoreService eventStore, CacheDatabaseManager db, InMemoryCache cache,
            IOptions<EndpointConfig> endpointConfig, ILogger<UsersController> logger)
        {
            _eventStore = eventStore;
            _db = db;
            _entityIndex = cache.Index<EntityIndex>();
            _userIndex = cache.Index<UserIndex>();
            _endpointConfig = endpointConfig.Value;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AllItemsResult<UserResult>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllAsync(UserQueryArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToGetAll(User.Identity))
                return Forbid();

            args = args ?? new UserQueryArgs();

            var roles = await Auth.GetUsersWithRolesAsync();

            try
            {
                // For filtering by role, we need to asynchronously retrieve the roles of each user
                // which is horribly inefficient, but the only solution for now
                // (only filtering by query text can be done beforehand)
                var users = _db.Database.GetCollection<User>(ResourceTypes.User.Name)
                    .AsQueryable()
                    .FilterIf(!string.IsNullOrEmpty(args.Query), user =>
                        user.FirstName.ToLower().Contains(args.Query.ToLower()) ||
                        user.LastName.ToLower().Contains(args.Query.ToLower()) ||
                        user.Email.ToLower().Contains(args.Query.ToLower()))
                    .ToList()
                    .Where(user => roles.ContainsKey(user.UserId)) // ignore orphaned users in DB
                    .Select(user => (user: user, roles: roles[user.UserId]))
                    .AsQueryable()
                    .FilterIf(!string.IsNullOrEmpty(args.Role), u => u.roles.Contains(args.Role))
                    .Sort(args.OrderBy,
                        ("firstName", x => x.user.FirstName),
                        ("lastName", x => x.user.LastName),
                        ("email", x => x.user.Email))
                    .PaginateAndSelect(args.Page, args.PageSize, u => new UserResult(u.user)
                    {
                        Roles = u.roles,
                        ProfilePicture = GenerateFileUrl(u.user.UserId)
                    });

                return Ok(users);
            }
            catch (InvalidSortKeyException e)
            {
                ModelState.AddModelError(nameof(args.OrderBy), e.Message);
                return BadRequest(ModelState);
            }
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

            var user = _db.Database.GetCollection<User>(ResourceTypes.User.Name)
                .AsQueryable()
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            var result = new UserResult(user)
            {
                Roles = await Auth.GetUserRolesAsStringAsync(user.UserId),
                ProfilePicture = GenerateFileUrl(userId)
            };

            return Ok(result);
        }

        [HttpGet("ByEmail/{email}")]
        [ProducesResponseType(typeof(UserResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByEmailAsync(string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToGetAll(User.Identity))
                return Forbid();

            var user = _db.Database.GetCollection<User>(ResourceTypes.User.Name)
                .AsQueryable()
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound();

            var result = new UserResult(user)
            {
                Roles = await Auth.GetUserRolesAsStringAsync(user.UserId),
                ProfilePicture = GenerateFileUrl(user.UserId)
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
        public async Task<IActionResult> PutAsync(string userId, [FromBody]UserUpdateArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            var oldUser = await EventStreamExtensions.GetCurrentEntityAsync<User>(_eventStore.EventStream, ResourceTypes.User, internalId);
            var changedUserArgs = new User(oldUser)
            {
                FirstName = args.FirstName,
                LastName = args.LastName,
            };
            await EntityManager.UpdateEntityAsync(_eventStore, oldUser, changedUserArgs, ResourceTypes.User, internalId,
                User.Identity.GetUserIdentity());
            return NoContent();
        }

        /// <summary>
        /// Creates a user in UserStore without registration in Auth0. Should only be used manually for
        /// maintenance/debugging purposes in cases where a user already exists in Auth0.
        /// </summary>
        [HttpPost("{userId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)] // User with same ID or email already exists
        public async Task<IActionResult> PostAsync(string userId, [FromBody]UserArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_userIndex.TryGetInternalId(userId, out var _))
                return StatusCode(409, new { Message = $"A user with ID '{userId}' already exists" });

            if (_userIndex.IsEmailInUse(args.Email))
                return StatusCode(409, new { Message = $"A user with email address '{args.Email}' already exists" });

            var user = new User
            {
                UserId = userId,
                Email = args.Email,
                FirstName = args.FirstName,
                LastName = args.LastName,
            };
            var id = _entityIndex.NextId(ResourceTypes.User);

            await EntityManager.CreateEntityAsync(_eventStore, user, ResourceTypes.User, id, User.Identity.GetUserIdentity());
            return Created($"{Request.Scheme}://{Request.Host}/api/Users/{userId}", userId);
        }

        /// <summary>
        /// Registers a new user in UserStore and Auth0 and assigns it the role 'Student'.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)] // User with same email already exists
        public async Task<IActionResult> RegisterAsync([FromBody]UserRegistrationArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_userIndex.IsEmailInUse(args.Email))
                return StatusCode(409, new { Message = $"A user with email address '{args.Email}' already exists" });

            try
            {
                // Step 1) Register user in Auth0
                var userId = await Auth.CreateUserAsync(args);

                try
                {
                    // Step 2) Register user in UserStore
                    var userArgs = new User
                    {
                        UserId = userId,
                        Email = args.Email,
                        FirstName = args.FirstName,
                        LastName = args.LastName
                    };

                    var id = _entityIndex.NextId(ResourceTypes.User);
                    await EntityManager.CreateEntityAsync(_eventStore, userArgs, ResourceTypes.User, id, User.Identity.GetUserIdentity());
                    return Created($"{Request.Scheme}://{Request.Host}/api/Users/{userId}", userId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        $"A new user with email address '{args.Email}' was registered in Auth0, but could not " +
                        $"be registered in UserStore. Users are now inconsistent/out of sync. To solve this, " +
                        $"delete the user in Auth0.");

                    throw; // In this case "500 Internal Server Error" is appropiate
                }
            }
            catch (ApiException e)
            {
                return StatusCode(e.ApiError.StatusCode, e.ApiError.Message);
            }
        }

        /// <summary>
        /// Updates the Auth0 roles of a user.
        /// </summary>
        [HttpPut("{userId}/Roles")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutRolesAsync(string userId, [FromBody]string[] roles)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invalidRoles = roles.Distinct().Where(s => !Enum.TryParse<UserRoles>(s, out _));

            foreach (var invalidRole in invalidRoles)
                ModelState.AddModelError("Roles", $"'{invalidRole}' is not a valid role");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userIndex.TryGetInternalId(userId, out var _))
                return NotFound();

            if (!UserPermissions.IsAllowedToChangeRoles(User.Identity))
                return Forbid();

            var actualRoles = roles.Distinct();
            await Auth.SetUserRolesAsync(userId, actualRoles);
            return NoContent();
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
