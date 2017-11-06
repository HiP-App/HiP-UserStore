using System.Collections.Generic;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;

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
            lock (_lockObject)
            {
                if (_auth0UserIds.TryGetValue(userId, out internalId))
                    return true;
            }

            internalId = -1;
            return false;
        }


        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case UserCreated ev:
                    lock (_lockObject)
                    {
                        _users.Add(ev.Id, new UserInfo());
                        _auth0UserIds.Add(ev.UserId, ev.Id);
                    }
                    break;

                case UserPhotoUploaded ev:
                    lock (_lockObject)
                        _users[ev.Id] = new UserInfo { ProfilePicturePath = ev.Path };
                    break;

                case UserPhotoDeleted ev:
                    lock (_lockObject)
                        _users[ev.Id].ProfilePicturePath = null;
                    break;
            }
        }

    }

    public class UserInfo
    {
        public string ProfilePicturePath { get; set; }
    }
}
