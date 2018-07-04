using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.Notifications.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Notifications;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : Controller
    {
        private EventStoreService _service;
        private EntityIndex _entityIndex;
        private CacheDatabaseManager _db;

        public NotificationsController(EventStoreService service, InMemoryCache cache, CacheDatabaseManager db)
        {
            _service = service;
            _entityIndex = cache.Index<EntityIndex>();
            _db = db;
        }

        /// <summary>
        /// Returns notifications of all types for the current user that are not marked as read
        /// </summary>
        /// <returns>List of unread notifications</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationResult>), 200)]
        [ProducesResponseType(403)]
        public IActionResult GetNotificationsForUser()
        {
            if (User.Identity.GetUserIdentity() == null)
                return Forbid();

            var query = _db.Database.GetCollection<NotificationBase>(ResourceTypes.Notification.Name).AsQueryable();
            string identity = User.Identity.GetUserIdentity();
            var notifications = query.Where(n => n.Recipient == identity && !n.IsRead).ToList();
            var resultList = notifications.Select(n => GetResultForNotification(n));
            return Ok(resultList);
        }

        /// <summary>
        /// Marks the notification with the provided id as read
        /// </summary>
        /// <param name="id">Id of the notification</param>
        /// <returns></returns>
        [HttpGet("MarkAsRead")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = _db.Database.GetCollection<NotificationBase>(ResourceTypes.Notification.Name).AsQueryable().FirstOrDefault(n => n.Id == id);

            if (notification == null)
                return NotFound($"A notification with the id {id} could not be found");

            var resourceType = GetResourceTypeForNotificationType(notification.Type);
            var args = notification.CreateNotificationArgs();
            var updatedArgs = notification.CreateNotificationArgs();
            updatedArgs.IsRead = true;

            await EntityManager.UpdateEntityAsync(_service, args, updatedArgs, resourceType, id, User.Identity.GetUserIdentity());

            return StatusCode(204);
        }


        private NotificationResult GetResultForNotification(NotificationBase notification)
        {
            switch (notification.Type)
            {
                case NotificationType.ReviewNotification:
                    return new ReviewNotificationResult(notification as ReviewNotification);

                default:
                    throw new NotSupportedException($"For the notification type {notification.GetType().Name} no result class can be found");
            }
        }

        private ResourceType GetResourceTypeForNotificationType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.ReviewNotification:
                    return NotificationTypes.ReviewNotification;

                default:
                    throw new NotSupportedException($"For the notification type {type} no ResourceType can be found");
            }
        }
    }
}
