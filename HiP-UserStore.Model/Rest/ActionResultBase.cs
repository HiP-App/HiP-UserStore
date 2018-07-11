using Action = PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Action;
using System;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using System.Runtime.Serialization;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    [KnownType(typeof(ExhibitVisitedActionResult))]
    [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
    public abstract class ActionResultBase
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Type { get; set; }

        public int EntityId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public ActionResultBase(Action action)
        {
            Id = action.Id;
            UserId = action.UserId;
            Type = action.TypeName;
            EntityId = action.EntityId;
            Timestamp = action.Timestamp;
        }
    }
}
