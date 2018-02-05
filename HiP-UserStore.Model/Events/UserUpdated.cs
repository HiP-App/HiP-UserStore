using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserUpdated : UserActivityBaseEvent, IUpdateEvent
    {
        public UserUpdateArgs Properties { get; set; }

        public override ResourceType GetEntityType() => ResourceTypes.User;
    }
}
