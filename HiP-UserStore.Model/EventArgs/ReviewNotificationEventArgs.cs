using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Notifications;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs
{
    public class ReviewNotificationEventArgs : NotificationBaseEventArgs
    {
        public int EntityId { get; set; }

        public ReviewEntityType EntityType { get; set; }

        public override NotificationBase CreateNotification()
        {
            return new ReviewNotification { Recipient = Recipient, EntityId = EntityId, IsRead = IsRead, Text = Text, EntityType = EntityType };
        }
    }
}
