using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaderbornUniversity.SILab.Hip.Notifications.Model;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Notifications
{
    public class ReviewNotificationResult : NotificationResult
    {
        public int EntityId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReviewEntityType EntityType { get; set; }

        public ReviewNotificationResult(ReviewNotification notification) : base(notification)
        {
            EntityId = notification.EntityId;
            EntityType = notification.EntityType;
        }
    }
}
