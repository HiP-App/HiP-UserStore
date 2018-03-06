using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs
{
    public class UserEventArgs
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [NestedObject]
        public StudentDetails StudentDetails { get; set; }

        /// <summary>
        /// The path to the actual profile picture file.
        /// </summary>
        public string ProfilePicturePath { get; set; }

        public UserEventArgs() { }

        public UserEventArgs(UserEventArgs other)
        {
            Email = other.Email;
            FirstName = other.FirstName;
            LastName = other.LastName;
            StudentDetails = other.StudentDetails;
            ProfilePicturePath = other.ProfilePicturePath;
            UserId = other.UserId;  
        }
    }
}
