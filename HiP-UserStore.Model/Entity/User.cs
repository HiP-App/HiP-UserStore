namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class User : ContentBase
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        /// <summary>
        /// The path to the actual profile picture file.
        /// </summary>
        public string ProfilePicturePath { get; set; }
    }
}
