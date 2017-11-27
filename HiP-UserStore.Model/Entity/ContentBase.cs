using System;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public abstract class ContentBase : IEntity<int>
    {
        public int Id { get; set; }

        public ContentStatus Status { get; set; }

        /// <summary>
        /// Owner of the content
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The date and time of the last modification.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
     }
}
