using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;

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

        public User (UserArgs2 args)
        {
            Email = args.Email;
            FirstName = args.FirstName;
            LastName = args.LastName;
            StudentDetails = args.StudentDetails;
            ProfilePicturePath = args.ProfilePicturePath;
        }

        public UserArgs2 CreateUserArgs2()
        {
            var args = new UserArgs2
            {
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                StudentDetails = StudentDetails,
                ProfilePicturePath = ProfilePicturePath
            };
            return args;
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
            CurrentSemester = args.CurrentSemester;
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
