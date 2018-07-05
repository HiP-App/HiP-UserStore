using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Notifications;

namespace PaderbornUniversity.SILab.Hip.UserStore.Rest
{
    public class ReviewNotificationArgs : NotificationBaseArgs
    {
        public int EntityId { get; set; }

        public ReviewEntityType EntityType { get; set; }
    }
}
