using MongoDB.Bson.Serialization.Attributes;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public class ActionType : BaseType
    {
        public static readonly ActionType ExhibitVisited = new ActionType("ExhibitVisited");

        [BsonConstructor]
        public ActionType(string name) : base(name) { }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj) => obj is ActionType other && Equals(other);

        public override bool Equals(BaseType other) => other is ActionType obj && Name == other.Name;

        public static bool operator ==(ActionType a, ActionType b) => a?.Equals(b) ?? b == null;

        public static bool operator !=(ActionType a, ActionType b) => !(a == b);
    }
}