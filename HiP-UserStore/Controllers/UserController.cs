﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly EventStoreClient _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly EntityIndex _entityIndex;
        private readonly UserIndex _userIndex;
        private readonly EndpointConfig _endpointConfig;
        private readonly ILogger<UserController> _logger;

        public UserController(EventStoreClient eventStore, CacheDatabaseManager db, InMemoryCache cache,
            IOptions<EndpointConfig> endpointConfig,
            ILogger<UserController> logger)
        {
            _eventStore = eventStore;
            _db = db;
            _entityIndex = cache.Index<EntityIndex>();
            _userIndex = cache.Index<UserIndex>();
            _endpointConfig = endpointConfig.Value;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserResult), 200)]
        public IActionResult GetById(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _db.Database.GetCollection<User>(ResourceType.User.Name)
                .AsQueryable()
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            var result = new UserResult(user)
            {
                Id = userId,
                ProfilePicture = GenerateFileUrl(userId)
            };

            return Ok(result);
        }

        [HttpGet("Me")]
        public IActionResult GetSelf() => GetById(User.Identity.GetUserIdentity());

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
                Timestamp = DateTimeOffset.Now
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

        private async Task InvalidateThumbnailCacheAsync(string userId)
        {
            if (!string.IsNullOrWhiteSpace(_endpointConfig.ThumbnailUrlPattern))
            {
                var url = string.Format(_endpointConfig.ThumbnailUrlPattern, userId);

                try
                {
                    using (var http = new HttpClient())
                    {
                        http.DefaultRequestHeaders.Add("Authorization", Request.Headers["Authorization"].ToString());
                        var response = await http.DeleteAsync(url);
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogWarning(e,
                        $"Request to clear thumbnail cache failed for user '{userId}'; " +
                        $"thumbnail service might return outdated images (request URL was '{url}').");
                }
            }
        }
    }
}
