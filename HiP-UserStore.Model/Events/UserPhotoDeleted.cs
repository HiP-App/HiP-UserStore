using PaderbornUniversity.SILab.Hip.EventSourcing;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserPhotoDeleted : UserActivityBaseEvent, IUpdateEvent
    {
        public override ResourceType GetEntityType() => ResourceTypes.User;
    }
}
