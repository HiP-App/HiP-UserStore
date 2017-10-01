using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.UserStore.Core.ReadModel;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Core.WriteModel;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using Microsoft.AspNetCore.StaticFiles;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        private readonly EventStoreClient _eventStore;
        private readonly CacheDatabaseManager _db;
        private readonly PhotoIndex _photoIndex;
        private readonly EntityIndex _entityIndex;
        private readonly UploadPhotoConfig _photoConfig;
        
        public PhotoController(EventStoreClient eventStore, CacheDatabaseManager db, InMemoryCache cache, IOptions<UploadPhotoConfig> photoConfig)
        {
            _eventStore = eventStore;
            _db = db;
            _photoIndex = cache.Index<PhotoIndex>();
            _entityIndex = cache.Index<EntityIndex>();
            _photoConfig = photoConfig.Value;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Get()
        {
            var media = _db.Database.GetCollection<Photo>(ResourceType.Photo.Name)
                 .AsQueryable()
                 .FirstOrDefault(x => x.UserId == User.Identity.GetUserIdentity());

            if (media?.Path == null || !System.IO.File.Exists(media.Path))
                return NotFound();

            new FileExtensionContentTypeProvider().TryGetContentType(media.Path, out string mimeType);
            mimeType = mimeType ?? "application/octet-stream";

            return File(new FileStream(media.Path, FileMode.Open), mimeType, Path.GetFileName(media.Path));
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([Required]IFormFile file)
        {
            if (file == null)
                ModelState.AddModelError("Argument Error", "File argument not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_photoIndex.ContainsUser(User.Identity))
                return StatusCode(409);

            var extension = file.FileName.Split('.').Last();

            // Checking supported extensions
            if (!_photoConfig.SupportedFormats.Contains(extension.ToLower()))
                return BadRequest(new { Message = $"Extension '{extension}' is not supported" });

            var filePath = SaveNewFile(file);
            var ev = new PhotoCreated
            {
                Id = _entityIndex.NextId(ResourceType.Photo),
                UserId = User.Identity.GetUserIdentity(),
                Path = filePath,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            return Created($"{Request.Scheme}://{Request.Host}/api/Photo/{ev.Id}", ev.Id);
        }

        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update([Required]IFormFile file)
        {
            if (file == null)
                ModelState.AddModelError("Argument Error", "File argument not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_photoIndex.ContainsUser(User.Identity))
                return NotFound();

            /// TODO: Add Permission System
            //if (!UserPermissions.IsAllowedToEdit(User.Identity, status, _entityIndex.Owner(ResourceType.Media, id)))
            //    return Forbid();

            var extension = file.FileName.Split('.').Last();

            // Checking supported extensions
            if (!_photoConfig.SupportedFormats.Contains(extension.ToLower()))
                return BadRequest(new { Message = $"Extension '{extension}' is not supported" });

            // Remove old file
            string oldFilePath = _photoIndex.GetFilePath(User.Identity);
            if (oldFilePath != null && System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var filePath = SaveNewFile(file);            
            var ev = new PhotoUpdated
            {
                Id = _entityIndex.Id(ResourceType.Photo, User.Identity),
                UserId = User.Identity.GetUserIdentity(),
                Path = filePath,
                Timestamp = DateTimeOffset.Now
            };

            await _eventStore.AppendEventAsync(ev);
            return StatusCode(204);
        }

        // Return path to file
        private string SaveNewFile(IFormFile file)
        {
            var fileDirectory = Path.Combine(_photoConfig.Path, User.Identity.GetUserIdentity().Replace('|', '@'));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, Path.GetFileName(file.FileName));

            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyToAsync(stream).Wait();
                }
            }
            return filePath;
        }

    }
}
