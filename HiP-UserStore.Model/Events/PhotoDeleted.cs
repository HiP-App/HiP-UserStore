namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class PhotoDeleted : UserActivityBaseEvent , IDeleteEvent
    {
        public override ResourceType GetEntityType() => ResourceType.Photo;
    }
}
