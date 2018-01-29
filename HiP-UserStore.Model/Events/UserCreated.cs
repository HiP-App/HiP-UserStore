using Newtonsoft.Json;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class UserCreated : UserActivityBaseEvent, ICreateEvent
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public UserArgs Properties { get; set; }

        public override ResourceType GetEntityType() => ResourceTypes.User;
    }
}
