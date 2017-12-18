using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public class ResourceType : IEquatable<ResourceType>
    {
        public static readonly ResourceType User = new ResourceType("User");
        public static readonly ResourceType Exhibits = new ResourceType("Exhibits");
        public static readonly ResourceType Routes = new ResourceType("Routes");
        public static readonly ResourceType Media = new ResourceType("Media");
        public static readonly ResourceType Tags = new ResourceType("Tags");
        public static readonly ResourceType ExhibitPages = new ResourceType("ExhibitsPages");

        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>
        [BsonElement]
        public string Name { get; }

        [BsonConstructor]
        public ResourceType(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name was null or empty", nameof(name));

            Name = name;
        }

        public override string ToString() => Name ?? "";

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj) => obj is ResourceType other && Equals(other);

        public bool Equals(ResourceType other) => Name == other.Name;

        public static bool operator ==(ResourceType a, ResourceType b) => a?.Equals(b) ?? b == null;

        public static bool operator !=(ResourceType a, ResourceType b) => !(a == b);
    }
}
