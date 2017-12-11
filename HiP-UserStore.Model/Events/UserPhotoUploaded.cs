namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserPhotoUploaded : UserActivityBaseEvent, IUpdateEvent
    {
        public string Path { get; set; }

        public override ResourceType GetEntityType() => ResourceType.User;
    }
}
