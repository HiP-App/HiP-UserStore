using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core
{
    public class ActivityIndex : IDomainIndex
    {
        private readonly Dictionary<string, Dictionary<ResourceType, DateTimeOffset>> _userTimestamps 
            = new Dictionary<string, Dictionary<ResourceType, DateTimeOffset>>();
        private readonly object _lockObject = new object();

        public void UpdateTimestamp (string userId, DateTimeOffset timestamp, ResourceType type)
        {
            lock (_lockObject)
            {
                if (_userTimestamps.ContainsKey(userId))
                {
                    if (_userTimestamps[userId].ContainsKey(type))
                    {
                        _userTimestamps[userId][type] = timestamp;
                    }
                }
                else
                {
                    _userTimestamps.Add(userId, InitTypeDict());
                }
            }
        }

        public void UpdateAllTimestamps (string userId, DateTimeOffset time)
        {
            if (_userTimestamps.ContainsKey(userId))
            {
                var tempTypeDict = new Dictionary<ResourceType, DateTimeOffset>(_userTimestamps[userId]);

                foreach (var type in tempTypeDict)
                {
                    _userTimestamps[userId][type.Key] = time;
                }
            }
        }

        public DateTimeOffset GetTimestamp (string userId, ResourceType type)
        {
            lock (_lockObject)
            {
                if (_userTimestamps.TryGetValue(userId, out var types))
                {
                    if (types.TryGetValue(type, out var timestamp))
                    {
                        return timestamp;
                    }
                }
                else
                {
                    _userTimestamps.Add(userId, InitTypeDict());
                }

                return DateTimeOffset.MinValue;
            }
        }

        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case PropertyChangedEvent ev:
                    var resourceType = ev.GetEntityType();
                    if (resourceType == ResourceTypes.User && ev.PropertyName == "UserId")
                    {
                        lock (_lockObject)
                        {
                            if (!_userTimestamps.ContainsKey(ev.Value.ToString()))
                            {
                                _userTimestamps.Add(ev.Value.ToString(), InitTypeDict());
                            }
                        }
                    }
                    break;
            }
        }

        public Dictionary<ResourceType, DateTimeOffset> InitTypeDict() =>
            new Dictionary<ResourceType, DateTimeOffset>
            {
                { ResourceTypes.Exhibit, DateTimeOffset.MinValue },
                { ResourceTypes.Route, DateTimeOffset.MinValue },
                { ResourceTypes.Tag, DateTimeOffset.MinValue },
                { ResourceTypes.Media, DateTimeOffset.MinValue },
                { ResourceTypes.ExhibitPage, DateTimeOffset.MinValue }
            };
    }
}
