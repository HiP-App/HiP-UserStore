using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class UserResult
    {
        /// <summary>
        /// The user ID (not the internally used integer-ID).
        /// </summary>
        public string Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string FullName => (FirstName == null && LastName == null) ? null : FirstName + ' ' + LastName;

        public StudentDetailsResult StudentDetails { get; set; }

        public IReadOnlyCollection<string> Roles { get; set; }

        /// <summary>
        /// URL from which the profile picture or a thumbnail of it can be obtained.
        /// </summary>
        public string ProfilePicture { get; set; }

        public UserResult(User user)
        {
            Id = user.UserId;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DisplayName = user.DisplayName;
            
            if (user.StudentDetails != null)
            {
                StudentDetails = new StudentDetailsResult
                {
                    Discipline = user.StudentDetails.Discipline,
                    CurrentDegree = user.StudentDetails.CurrentDegree,
                    CurrentSemester = user.StudentDetails.CurrentSemester
                };
            }
        }
    }

    public class StudentDetailsResult
    {
        public string Discipline { get; set; }

        public string CurrentDegree { get; set; }

        public int CurrentSemester { get; set; }
    }

    public class StudentDetailsArgs
    {
        [Required]
        public string Discipline { get; set; }

        [Required]
        public string CurrentDegree { get; set; }

        [Range(1, 50)]
        public int? CurrentSemester { get; set; }
    }
}
