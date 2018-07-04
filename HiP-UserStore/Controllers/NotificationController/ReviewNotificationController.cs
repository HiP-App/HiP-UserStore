using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.Notifications.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Model;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers.NotificationController
{
    public class ReviewNotificationController : NotificationBaseController<ReviewNotificationArgs>
    {
        public ReviewNotificationController(EventStoreService eventStore, InMemoryCache cache) : base(eventStore, cache)
        {
        }

        protected override ResourceType ResourceType => NotificationTypes.ReviewNotification;
    }
}
