using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    /// <summary>
    /// Arguments for creating a user in both, UserStore and Auth0.
    /// </summary>
    /// <remarks>
    /// To prevent accidentally serializing and storing the password in the event store,
    /// this type is intentionally not derived from <see cref="UserArgs"/>.
    /// </remarks>
    public class UserRegistrationArgs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
