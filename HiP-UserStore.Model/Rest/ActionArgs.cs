using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public abstract class ActionArgs
    {
        /// <summary>
        /// Id of entity, which was completed
        /// </summary>
        [Required]
        public int EntityId { get; set; }

        public abstract Action CreateAction();
    }

    public abstract class ActionsArgs
    {
        /// <summary>
        /// Id of entity, which was completed
        /// </summary>
        [Required]
        public List<int> EntityIds { get; set; }

        public abstract List<Action> CreateActions();

        public abstract List<ActionArgs> ToListActionArgs();
    }
}
