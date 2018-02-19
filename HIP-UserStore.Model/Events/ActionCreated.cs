using Newtonsoft.Json;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    public class ActionCreated : UserActivityBaseEvent, ICreateEvent
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ActionArgs Properties { get; set; }
        
        public override ResourceType GetEntityType() => ResourceTypes.Action;
    }
}
