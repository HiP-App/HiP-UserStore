using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Actions;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions
{
    public class ExhibitVisitedActionArgs : ActionArgs
    {
        public override Action CreateAction() => new ExhibitVisitedAction(this);
    }
    public class ExhibitVisitedActionsArgs : ActionsArgs
    {
        public override List<Action> CreateActions() => ExhibitVisitedAction.Factory(this);

        public override List<ActionArgs> ToListActionArgs()
        {
            var result = new List<ActionArgs>();
            foreach (var entityId in EntityIds)
            {
                result.Add(new ExhibitVisitedActionArgs() { EntityId = entityId });
            }
            return result;
        }
    }
}
