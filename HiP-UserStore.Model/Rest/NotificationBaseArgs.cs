using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public abstract class NotificationBaseArgs
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public string Recipient { get; set; }
    }
}
