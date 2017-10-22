using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Authorize]
    [Route("api/Users")]
    public class PhotoController : Controller
    {
        private readonly EventStoreClient _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly UserIndex _userIndex;
        private readonly UploadPhotoConfig _photoConfig;
        private readonly EndpointConfig _endpointConfig;
        private readonly ILogger<PhotoController> _logger;

        public PhotoController(EventStoreClient eventStore, CacheDatabaseManager db, InMemoryCache cache,
            IOptions<UploadPhotoConfig> photoConfig, IOptions<EndpointConfig> endpointConfig,
            ILogger<PhotoController> logger)
        {
            _eventStore = eventStore;
            _db = db;
            _userIndex = cache.Index<UserIndex>();
            _photoConfig = photoConfig.Value;
            _endpointConfig = endpointConfig.Value;
            _logger = logger;
        }

        [HttpGet("{userId}/Photo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Get(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UserPermissions.IsAllowedToGetPhoto(User.Identity, userId))
                return Forbid();

            var user = _db.Database.GetCollection<User>(ResourceType.User.Name)
                 .AsQueryable()
                 .FirstOrDefault(x => x.UserId == userId);

            if (user?.ProfilePicturePath == null || !System.IO.File.Exists(user.ProfilePicturePath))
                return NotFound();

            new FileExtensionContentTypeProvider().TryGetContentType(user.ProfilePicturePath, out var mimeType);
            mimeType = mimeType ?? "application/octet-stream";

            return File(new FileStream(user.ProfilePicturePath, FileMode.Open), mimeType, Path.GetFileName(user.ProfilePicturePath));
        }

        [HttpPut("{userId}/Photo")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Upload([Required]IFormFile file, string userId)
        {
            if (file == null)
                ModelState.AddModelError("Argument Error", "File argument not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            // ReSharper disable once PossibleNullReferenceException (we already handled file == null)
            var extension = Path.GetExtension(file.FileName).Replace(".", "");

            // Checking supported extensions
            if (!_photoConfig.SupportedFormats.Contains(extension.ToLower()))
                return BadRequest(new { Message = $"Extension '{extension}' is not supported" });

            // Remove old file
            var oldFilePath = _userIndex.GetProfilePicturePath(userId);
            if (oldFilePath != null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var filePath = SaveNewFile(file, userId);

            var ev = new UserPhotoUploaded
            {
                Id = internalId,
                UserId = userId,
                Path = filePath,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            await InvalidateThumbnailCacheAsync(userId);
            return StatusCode(204);
        }

        [HttpDelete("{userId}/Photo")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            // Remove photo
            var directoryPath = Path.GetDirectoryName(_userIndex.GetProfilePicturePath(userId));
            if (directoryPath != null && Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            var ev = new UserPhotoDeleted
            {
                Id = internalId,
                UserId = userId,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            await InvalidateThumbnailCacheAsync(userId);
            return StatusCode(204);
        }

        // Return path to file
        private string SaveNewFile(IFormFile file, string userId)
        {
            var fileDirectory = Path.Combine(_photoConfig.Path, userId.Replace('|', '@'));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, Path.GetFileName(file.FileName));

            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return filePath;
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
