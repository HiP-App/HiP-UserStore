using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Notifications
{
    public class ReviewNotification : NotificationBase
    {
        public ReviewNotification()
        {
        }

        public ReviewNotification(ReviewNotificationEventArgs args) : base(args)
        {
            EntityId = args.EntityId;
            EntityType = args.EntityType;
        }

        /// <summary>
        /// The id of the exhibit the review belongs to
        /// </summary>
        public int EntityId { get; set; }

        public ReviewEntityType EntityType { get; set; }

        public override NotificationType Type => NotificationType.ReviewNotification;

        public override NotificationBaseEventArgs CreateNotificationArgs()
        {
            return new ReviewNotificationEventArgs { EntityId = EntityId, Recipient = Recipient, Text = Text, IsRead = IsRead, EntityType = EntityType };
        }
    }
}
