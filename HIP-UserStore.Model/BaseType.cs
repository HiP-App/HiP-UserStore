using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public abstract class BaseType : IEquatable<BaseType>
    {
        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>

        [BsonElement]
        public string Name { get; }

        [BsonConstructor]
        public BaseType(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name was null or empty", nameof(name));

            Name = name;
        }

        public override string ToString() => Name ?? "";

        public override int GetHashCode() => Name.GetHashCode();

        public abstract bool Equals(BaseType other);
    }
}
