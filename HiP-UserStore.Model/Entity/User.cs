using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class User : ContentBase
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [NestedObject]
        public StudentDetails StudentDetails { get; set; }

        /// <summary>
        /// The path to the actual profile picture file.
        /// </summary>
        public string ProfilePicturePath { get; set; }

        public User()
        {
        }

        public User(User other)
        {
            Email = other.Email;
            FirstName = other.FirstName;
            LastName = other.LastName;
            StudentDetails = other.StudentDetails;
            ProfilePicturePath = other.ProfilePicturePath;
            UserId = other.UserId;
            Id = other.Id;
            Timestamp = other.Timestamp;
        }
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
