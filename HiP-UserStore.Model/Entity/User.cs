using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class User : ContentBase
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public StudentDetails StudentDetails { get; set; }

        /// <summary>
        /// The path to the actual profile picture file.
        /// </summary>
        public string ProfilePicturePath { get; set; }

        public User()
        {
        }

        public User(UserEventArgs transferObject)
        {
            Email = transferObject.Email;
            FirstName = transferObject.FirstName;
            LastName = transferObject.LastName;
            StudentDetails = transferObject.StudentDetails;
            ProfilePicturePath = transferObject.ProfilePicturePath;
            UserId = transferObject.UserId;
        }

        public UserEventArgs CreateUserTransferObject()
        => new UserEventArgs()
        {
            UserId = UserId,
            Email = Email,
            FirstName = FirstName,
            LastName = LastName,
            ProfilePicturePath = ProfilePicturePath,
            StudentDetails = StudentDetails
        };

    }

    public class StudentDetails
    {
        public string Discipline { get; set; }

        public string CurrentDegree { get; set; }

        public int CurrentSemester { get; set; }

        public StudentDetails()
        {
        }

        public StudentDetails(StudentDetailsArgs args)
        {
            Discipline = args.Discipline;
            CurrentDegree = args.CurrentDegree;
            CurrentSemester = args.CurrentSemester ?? new int();
        }

        public StudentDetailsArgs CreateStudentDetailsArgs()
        {
            var args = new StudentDetailsArgs
            {
                Discipline = Discipline,
                CurrentDegree = CurrentDegree,
                CurrentSemester = CurrentSemester
            };
            return args;
        }
    }
}
