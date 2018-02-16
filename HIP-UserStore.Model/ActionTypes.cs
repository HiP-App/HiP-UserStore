using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public static class ActionTypes
    {
        public static ResourceType ExhibitVisited { get; private set; }
        /// <summary>
        /// Initializes the fieldd
        /// </summary>
        public static void Initialize()
        {
           ExhibitVisited = ResourceType.Register(nameof(ExhibitVisited) , typeof(ExhibitVisitedActionArgs), ResourceTypes.Action);
        }
    }
}
