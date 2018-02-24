using Action = PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Action;
using System;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using System.Runtime.Serialization;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    [KnownType(typeof(ExhibitVisitedActionResult))]
    public abstract class ActionResult
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Type { get; set; }

        public int EntityId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public ActionResult(Action action)
        {
            Id = action.Id;
            UserId = action.UserId;
            Type = action.TypeName;
            EntityId = action.EntityId;
            Timestamp = action.Timestamp;
        }
    }
}
