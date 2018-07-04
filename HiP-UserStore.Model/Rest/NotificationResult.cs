using Newtonsoft.Json;
using NJsonSchema.Converters;
using PaderbornUniversity.SILab.Hip.Notifications.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Notifications;
using System;
using System.Runtime.Serialization;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    /// <summary>
    /// This class functions as a base class for a specifc notificatoin result class and contains basic properties that every notification has 
    /// </summary>
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    [KnownType(typeof(ReviewNotificationResult))]
    public abstract class NotificationResult
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public bool IsRead { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public NotificationResult(NotificationBase notification)
        {
            Id = notification.Id;
            Type = notification.Type.ToString();
            IsRead = notification.IsRead;
            Text = notification.Text;
            Timestamp = notification.Timestamp;
        }
    }
}
