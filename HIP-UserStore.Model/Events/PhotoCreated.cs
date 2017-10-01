namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class PhotoCreated : UserActivityBaseEvent, ICreateEvent
    {
        public string Path { get; set; }

        public override ResourceType GetEntityType() => ResourceType.Photo;
    }
}
