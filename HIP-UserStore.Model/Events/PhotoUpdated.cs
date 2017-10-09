namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class PhotoUploaded : UserActivityBaseEvent, IUpdateEvent
    {
        public string Path { get; set; }

        public override ResourceType GetEntityType() => ResourceType.Photo;
    }
}
