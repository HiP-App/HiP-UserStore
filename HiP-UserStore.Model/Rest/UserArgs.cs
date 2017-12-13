using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    /// <summary>
    /// Arguments for creating a user in UserStore.
    /// </summary>
    public class UserArgs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public UserArgs()
        {
        }

        public UserArgs(UserRegistrationArgs args)
        {
            Email = args.Email;
            FirstName = args.FirstName;
            LastName = args.LastName;
        }
    }
}
