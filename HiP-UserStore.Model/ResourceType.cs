using MongoDB.Bson.Serialization.Attributes;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public class ResourceType : BaseType
    {
        public static readonly ResourceType User = new ResourceType("User");
        public static readonly ResourceType Exhibits = new ResourceType("Exhibits");
        public static readonly ResourceType Routes = new ResourceType("Routes");
        public static readonly ResourceType Media = new ResourceType("Media");
        public static readonly ResourceType Tags = new ResourceType("Tags");
        public static readonly ResourceType ExhibitPages = new ResourceType("ExhibitsPages");
        public static readonly ResourceType Action = new ResourceType("Action");


        [BsonConstructor]
        public ResourceType(string name) : base(name){    }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj) => obj is ResourceType other && Equals(other);

        public override bool Equals(BaseType other) => other is ResourceType obj && Name == other.Name;

        public static bool operator ==(ResourceType a, ResourceType b) => a?.Equals(b) ?? b == null;

        public static bool operator !=(ResourceType a, ResourceType b) => !(a == b);
    }
}
