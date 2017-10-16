namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserCreated : UserActivityBaseEvent, ICreateEvent
    {
        // In the future, we probably need "UserArgs" containing user details (name, email, ...)

        public override ResourceType GetEntityType() => ResourceType.User;
    }
}
