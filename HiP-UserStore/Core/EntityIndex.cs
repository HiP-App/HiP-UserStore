using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core
{
    public class EntityIndex : IDomainIndex
    {
        private readonly Dictionary<ResourceType, EntityTypeInfo> _types = new Dictionary<ResourceType, EntityTypeInfo>();
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets a new, never-used-before ID for a new entity of the specified type.
        /// </summary>
        public int NextId(ResourceType entityType)
        {
            lock (_lockObject)
            {
                var info = GetOrCreateEntityTypeInfo(entityType);
                return ++info.MaximumId;
            }
        }

        /// <summary>
        /// Get UserId of an entity owner
        /// </summary>
        public string Owner(ResourceType entityType, int id)
        {
            var info = GetOrCreateEntityTypeInfo(entityType);

            if (info.Entities.TryGetValue(id, out var entity))
                return entity.UserId;

            return null;
        }

        /// <summary>
        /// Determines whether an entity with the specified type and ID exists.
        /// </summary>
        public bool Exists(ResourceType entityType, int id)
        {
            lock (_lockObject)
            {
                var info = GetOrCreateEntityTypeInfo(entityType);
                return info.Entities.ContainsKey(id);
            }
        }

        public void ApplyEvent(IEvent e)
        {
            switch (e)
            {
                case ICreateEvent ev:
                    lock (_lockObject)
                    {
                        var owner = (ev as UserActivityBaseEvent)?.UserId;
                        var info = GetOrCreateEntityTypeInfo(ev.GetEntityType());
                        info.MaximumId = Math.Max(info.MaximumId, ev.Id);
                        info.Entities.Add(ev.Id, new EntityInfo { UserId = owner });
                    }
                    break;

                case IDeleteEvent ev:
                    lock (_lockObject)
                    {
                        var info3 = GetOrCreateEntityTypeInfo(ev.GetEntityType());
                        info3.Entities.Remove(ev.Id);
                    }
                    break;
            }
        }

        private EntityTypeInfo GetOrCreateEntityTypeInfo(ResourceType entityType)
        {
            if (_types.TryGetValue(entityType, out var info))
                return info;

            return _types[entityType] = new EntityTypeInfo();
        }

        class EntityTypeInfo
        {
            /// <summary>
            /// The largest ID ever assigned to an entity of the type.
            /// </summary>
            public int MaximumId { get; set; } = -1;

            /// <summary>
            /// Stores only the most basic information about all entities of the type.
            /// It is assumed that this easily fits in RAM.
            /// </summary>
            public Dictionary<int, EntityInfo> Entities { get; } = new Dictionary<int, EntityInfo>();
        }

        class EntityInfo
        {
            /// <summary>
            /// Owner of the entity
            /// </summary>
            public string UserId { get; set; }
        }
    }
}
