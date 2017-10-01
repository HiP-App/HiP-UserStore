namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class PhotoUpdated : UserActivityBaseEvent, IUpdateEvent
    {
        public string Path { get; set; }

        public override ResourceType GetEntityType() => ResourceType.Photo;
    }
}
