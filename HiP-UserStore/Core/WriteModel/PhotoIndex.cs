using System.Collections.Generic;
using System.Security.Principal;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core.WriteModel
{
    public class PhotoIndex : IDomainIndex
    {
        /// <summary>
        /// Key = UserId , Value = PhotoInfo 
        /// </summary>
        private readonly Dictionary<string, PhotoInfo> _photo = new Dictionary<string, PhotoInfo>();
        private readonly object _lockObject = new object();

        public string GetFilePath(IIdentity User)
        {
            lock (_lockObject)
            {
                if (_photo.TryGetValue(User.GetUserIdentity(), out var photoInfo))
                    return photoInfo.Path;
                return null;
            }
        }

        public bool ContainsUser(IIdentity User)
        {
            lock (_lockObject)
                return _photo.ContainsKey(User.GetUserIdentity());
        }

        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case PhotoUploaded ev:
                    lock (_lockObject)
                        _photo[ev.UserId]= new PhotoInfo() { Path = ev.Path };
                    break;

                case PhotoDeleted ev:
                    lock (_lockObject)
                        _photo.Remove(ev.UserId);
                    break;
            }
        }

    }
    public class PhotoInfo
    {
        public string Path { get; set; }
    }
 }
