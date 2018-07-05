using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Rest
{
    public abstract class NotificationBaseArgs
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public string Recipient { get; set; }
    }
}
