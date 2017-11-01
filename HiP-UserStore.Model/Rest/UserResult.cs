﻿using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;

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

        public string FullName => (FirstName == null && LastName == null) ? null : FirstName + ' ' + LastName;

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
        }
    }
}
