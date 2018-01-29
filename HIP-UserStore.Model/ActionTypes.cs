using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public static class ActionTypes
    {
        public static ResourceType ExhibitVisitedAction;
        /// <summary>
        /// Initializes the fieldd
        /// </summary>
        public static void Initialize()
        {
           ExhibitVisitedAction = ResourceType.Register(nameof(ExhibitVisitedAction), typeof(ExhibitVisitedActionArgs));
        }
    }
}
