using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Notifications;
using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    /// <summary>
    /// This class serves as a notification with basic information (e.g. text, referrer). 
    /// More specific notification classes can be derived from this class
    /// </summary>
    public abstract class NotificationBase : ContentBase
    {
        public NotificationBase() { }
        public NotificationBase(NotificationBaseEventArgs args)
        {
            Text = args.Text;
            Recipient = args.Recipient;
            IsRead = args.IsRead;
        }

        /// <summary>
        /// Basic information of the notification
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The id of the user that receives this notification
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Determines if the notifiation has been read by the user
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Type of the notification 
        /// </summary>
        public abstract NotificationType Type { get; }

        public abstract NotificationBaseEventArgs CreateNotificationArgs();
    }
}
