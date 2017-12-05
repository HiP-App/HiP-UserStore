using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    /// <summary>
    /// Arguments for creating a user.
    /// </summary>
    public class UserArgs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
