using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.EventSourcing;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserUpdated : UserActivityBaseEvent, IUpdateEvent
    {
        public UserUpdateArgs Properties { get; set; }

        public override ResourceType GetEntityType() => ResourceTypes.User;
    }
}
