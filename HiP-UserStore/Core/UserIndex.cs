using System.Collections.Generic;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core
{
    public class UserIndex : IDomainIndex
    {
        /// <summary>
        /// Key = Auth0 user ID, Value = UserInfo
        /// </summary>
        private readonly Dictionary<string, UserInfo> _users = new Dictionary<string, UserInfo>();
        private readonly object _lockObject = new object();

        public string GetProfilePicturePath(string userId)
        {
            lock (_lockObject)
            {
                if (_users.TryGetValue(userId, out var photoInfo))
                    return photoInfo.ProfilePicturePath;
                return null;
            }
        }

        public bool Exists(string userId)
        {
            lock (_lockObject)
                return _users.ContainsKey(userId);
        }

        /// <summary>
        /// Given an Auth0 user ID, returns the internally used integer user ID.
        /// </summary>
        public bool TryGetInternalId(string userId, out int id)
        {
            if (_users.TryGetValue(userId, out var info))
            {
                id = info.InternalId;
                return true;
            }

            id = -1;
            return false;
        }

        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case UserCreated ev:
                    lock (_lockObject)
                        _users.Add(ev.UserId, new UserInfo { InternalId = ev.Id });
                    break;

                case UserPhotoUploaded ev:
                    lock (_lockObject)
                        _users[ev.UserId] = new UserInfo { ProfilePicturePath = ev.Path };
                    break;

                case UserPhotoDeleted ev:
                    lock (_lockObject)
                        _users[ev.UserId].ProfilePicturePath = null;
                    break;
            }
        }

    }

    public class UserInfo
    {
        /// <summary>
        /// The user's ID that is internally used in UserStore.
        /// </summary>
        public int InternalId { get; set; }

        public string ProfilePicturePath { get; set; }
    }
}
