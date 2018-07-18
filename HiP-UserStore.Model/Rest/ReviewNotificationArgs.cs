using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Notifications;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class ReviewNotificationArgs : NotificationBaseArgs
    {
        public int EntityId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]

        public ReviewEntityType EntityType { get; set; }
    }
}
