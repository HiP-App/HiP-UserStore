using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class UserArgs2
    {
        public string UserId { get; set; }

        public string Email { set; get; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        [NestedObject]
        public StudentDetails StudentDetails { get; set; }

        public string ProfilePicturePath { get; set; }

        public UserArgs2 ()
        {
        }

        public UserArgs2(UserArgs2 args)
        {
            UserId = args.UserId;
            Email = args.Email;
            FirstName = args.FirstName;
            LastName = args.LastName;
            Password = args.Password;
            StudentDetails = args.StudentDetails;
            ProfilePicturePath = args.ProfilePicturePath;
        }
    }
}
