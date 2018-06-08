using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class UserQueryArgs : QueryArgs
    {
        /// <summary>
        /// Restricts the response result to users having the specified role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Restricts the response to users having an email that begins with the specified string
        /// </summary>
        public string EmailBeginning { get; set; }
                
        /// <summary>
        /// If set, only those users, with their IDs specified, are included in the response.
        /// If null, all IDs are included in the response.
        /// </summary>
        public List<string> UsersIdsToBeIncluded { get; set; }
    }
}
