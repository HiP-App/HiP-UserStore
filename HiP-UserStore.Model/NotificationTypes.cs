using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.Notifications.Model.EventArgs;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public static class NotificationTypes
    {
        public static ResourceType ReviewNotification { get; private set; }

        /// <summary>
        /// Initializes the fields
        /// </summary>
        public static void Initialize()
        {
            ReviewNotification = ResourceType.Register(nameof(ReviewNotification), typeof(ReviewNotificationEventArgs), ResourceTypes.Notification);
        }
    }
}
