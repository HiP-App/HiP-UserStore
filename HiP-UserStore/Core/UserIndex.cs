using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core
{
    public class UserIndex : IDomainIndex
    {
        /// <summary>
        /// Key = internal user ID, Value = UserInfo
        /// </summary>
        private readonly Dictionary<int, UserInfo> _users = new Dictionary<int, UserInfo>();
        private readonly Dictionary<string, int> _auth0UserIds = new Dictionary<string, int>();
        private readonly object _lockObject = new object();

        public string GetProfilePicturePath(int userId)
        {
            lock (_lockObject)
            {
                if (_users.TryGetValue(userId, out var photoInfo))
                    return photoInfo.ProfilePicturePath;
                return null;
            }
        }

        /// <summary>
        /// Given an Auth0 user ID, returns the internally used integer user ID.
        /// </summary>
        public bool TryGetInternalId(string userId, out int internalId)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            lock (_lockObject)
            {
                if (_auth0UserIds.TryGetValue(userId, out internalId))
                    return true;
            }

            internalId = -1;
            return false;
        }

        /// <summary>
        /// Checks whether a user with the specified email address exists.
        /// </summary>
        public bool IsEmailInUse(string email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            return _users.Any(u => u.Value.Email == email);
        }

        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case UserCreated ev:
                    lock (_lockObject)
                    {
                        _users.Add(ev.Id, new UserInfo { Email = ev.Properties?.Email });
                        _auth0UserIds.Add(ev.UserId, ev.Id);
                    }
                    break;

                case UserPhotoUploaded ev:
                    lock (_lockObject)
                    {
                        if (_users.TryGetValue(ev.Id, out var info))
                            info.ProfilePicturePath = ev.Path;
                    }
                    break;

                case UserPhotoDeleted ev:
                    lock (_lockObject)
                    {
                        if (_users.TryGetValue(ev.Id, out var info))
                            info.ProfilePicturePath = null;
                    }
                    break;
            }
        }

    }

    public class UserInfo
    {
        public string Email { get; set; }

        public string ProfilePicturePath { get; set; }
    }
}
