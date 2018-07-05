using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs
{
    public abstract class NotificationBaseEventArgs
    {
        public string Text { get; set; }

        public string Recipient { get; set; }

        public bool IsRead { get; set; }

        public abstract NotificationBase CreateNotification();
    }
}
