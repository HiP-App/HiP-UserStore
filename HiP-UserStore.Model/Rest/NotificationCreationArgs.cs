using Newtonsoft.Json.Linq;
using PaderbornUniversity.SILab.Hip.Notifications.Model;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class NotificationCreationArgs
    {
        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public JObject NotificationArgs { get; set; }

    }
}
