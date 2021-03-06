﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;
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
        private readonly EventStoreService _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly UserIndex _userIndex;
        private readonly UploadPhotoConfig _photoConfig;
        private readonly PredefinedAvatarsConfig _avatarConfig;
        private readonly EndpointConfig _endpointConfig;
        private readonly ILogger<PhotoController> _logger;

        public PhotoController(EventStoreService eventStore, CacheDatabaseManager db, InMemoryCache cache,
            IOptions<UploadPhotoConfig> photoConfig,
            IOptions<PredefinedAvatarsConfig> avatarConfig,
            IOptions<EndpointConfig> endpointConfig,
            ILogger<PhotoController> logger)
        {
            _eventStore = eventStore;
            _db = db;
            _userIndex = cache.Index<UserIndex>();
            _photoConfig = photoConfig.Value;
            _avatarConfig = avatarConfig.Value;
            _endpointConfig = endpointConfig.Value;
            _logger = logger;
        }

        [HttpGet("{userId}/Photo")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Get(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToGetPhoto(User.Identity, userId))
                return Forbid();

            var user = _db.Database.GetCollection<User>(ResourceTypes.User.Name)
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
            var oldFilePath = _userIndex.GetProfilePicturePath(internalId);
            if (oldFilePath != null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var filePath = SaveNewFile(file, userId);

            var oldUser = await _eventStore.EventStream.GetCurrentEntityAsync<UserEventArgs>(ResourceTypes.User, internalId);
            var newUser = new UserEventArgs(oldUser)
            {
                ProfilePicturePath = filePath
            };

            await EntityManager.UpdateEntityAsync(_eventStore, oldUser, newUser, ResourceTypes.User, internalId, User.Identity.GetUserIdentity());
            await InvalidateThumbnailCacheAsync(userId);
            return NoContent();
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
            var directoryPath = Path.GetDirectoryName(_userIndex.GetProfilePicturePath(internalId));
            // if path points to predefined Avatar then no deletion
            var avatarPath = Path.GetDirectoryName(_avatarConfig.Path + "/");
            if (directoryPath != null && Directory.Exists(directoryPath) && !directoryPath.Contains(avatarPath))
                Directory.Delete(directoryPath, true);

            var ev = new UserPhotoDeleted
            {
                Id = internalId,
                UserId = User.Identity.GetUserIdentity(),
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            await InvalidateThumbnailCacheAsync(userId);
            return NoContent();
        }


        [HttpGet("PredefinedAvatars")]
        [ProducesResponseType(typeof(string[]), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetPredefinedPictureList()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            DirectoryInfo directoryInfo = new DirectoryInfo(_avatarConfig.Path + "/");
            FileInfo[] fileInfoArray = directoryInfo.GetFiles();
            string[] fileInfoStringArray = new string[fileInfoArray.Length];
            for (int i = 0; i < fileInfoArray.Length; i++)
            {
                fileInfoStringArray[i] = Path.GetFileName(fileInfoArray[i].FullName);
            }

            return Ok(fileInfoStringArray);
        }

        [HttpGet("PredefinedAvatars/{id}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetPredefinedPictureById(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!System.IO.File.Exists(_avatarConfig.Path + "/"  + id))
                return NotFound();

            new FileExtensionContentTypeProvider().TryGetContentType(_avatarConfig.Path + "/"  + id, out var mimeType);
            mimeType = mimeType ?? "application/octet-stream";

            return File(new FileStream(_avatarConfig.Path + "/"  + id, FileMode.Open), mimeType, Path.GetFileName(_avatarConfig.Path + "/"  + id));
        }


        [HttpPut("{userId}/Avatar")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetPredefinedAvatar([Required]string avatarId, string userId)
        {
            if (avatarId == null)
                ModelState.AddModelError("Argument Error", "Avatar ID not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UserPermissions.IsAllowedToModify(User.Identity, userId))
                return Forbid();

            if (!_userIndex.TryGetInternalId(userId, out var internalId))
                return NotFound();

            // Remove old file
            var oldFilePath = _userIndex.GetProfilePicturePath(internalId);
            // must not point to predefined Avatars path
            if (oldFilePath != null && System.IO.File.Exists(oldFilePath) && !oldFilePath.Contains(Path.GetDirectoryName(_avatarConfig.Path + "/")))
                System.IO.File.Delete(oldFilePath);

            var oldUser = await _eventStore.EventStream.GetCurrentEntityAsync<UserEventArgs>(ResourceTypes.User, internalId);
            var newUser = new UserEventArgs(oldUser)
            {
                ProfilePicturePath = Path.GetDirectoryName(_avatarConfig.Path + "/") + "\\" + avatarId
            };

            await EntityManager.UpdateEntityAsync(_eventStore, oldUser, newUser, ResourceTypes.User, internalId, User.Identity.GetUserIdentity());
            await InvalidateThumbnailCacheAsync(userId);
            return NoContent();
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
