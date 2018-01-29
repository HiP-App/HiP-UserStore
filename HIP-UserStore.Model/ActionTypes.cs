using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;


namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public class ActionTypes
    {
        public static ResourceType ExhibitVisited;

        public static void Initialize()
        {
            ExhibitVisited = ResourceType.Register(nameof(ExhibitVisited), typeof(ExhibitVisitedActionArgs));
        }
    }   
}

