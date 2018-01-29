using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.EventSourcing;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    /// <summary>
    /// Represents changes to the student details of a user.
    /// </summary>
    public class UserStudentDetailsUpdated : UserActivityBaseEvent
    {
        public StudentDetailsArgs Properties { get; set; }

        public override ResourceType GetEntityType() => ResourceTypes.User;
    }
}
