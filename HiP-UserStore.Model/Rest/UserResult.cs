using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class UserResult
    {
        /// <summary>
        /// The user ID (not the internally used integer-ID).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// URL from which the profile picture or a thumbnail of it can be obtained.
        /// </summary>
        public string ProfilePicture { get; set; }

        public UserResult(User user)
        {
            Id = user.UserId;
        }
    }
}
