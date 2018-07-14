using System;
using System.Linq;
using System.Security.Principal;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class UserPermissions
    {
        public static bool IsAllowedToGetPhoto(IIdentity identity, string ownerId)
        {
            // Profile pictures are public (for now) - anyone can access anyone else's photo
            return true;
        }

        public static bool IsAllowedToModify(IIdentity identity, string ownerId)
        {
            // Users can change their own details & profile picture, admins/supervisors can modify any user
            return (identity.GetUserIdentity() == ownerId) || CheckRoles(identity);
        }

        public static bool IsAllowedToGet(IIdentity identity, string requestedUserId)
        {
            // Users can get info about themselves, admins/supervisors can get info of any user
            return (identity.GetUserIdentity() == requestedUserId) || CheckRoles(identity);
        }

        public static bool IsAllowedToGetHistory(IIdentity identity, string ownerId)
        {
            // The entity owner as well as supervisors and administrators are allowed
            return (ownerId == identity.GetUserIdentity()) || CheckRoles(identity);
        }

        public static bool IsAllowedToGetAll(IIdentity identity) => CheckRoles(identity);

        /// <summary>
        /// Only Supervisor and Administrator can get all information about Users Actions
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool IsAllowedToGetAllActions(IIdentity identity) => CheckRoles(identity);


        public static bool IsAllowedToChangeRoles(IIdentity identity) => CheckRoles(identity, UserRoles.Administrator);

        // Check if the user has the nessesary roles
        static bool CheckRoles(IIdentity identity, UserRoles allowedToProceed = UserRoles.Administrator | UserRoles.Supervisor)
        {
            return identity.GetUserRoles()
                           .Any(x => (Enum.TryParse(x.Value, out UserRoles role) && (allowedToProceed & role) != 0)); // Bitwise AND
        }

        public static bool IsAllowedToModifyNotification(IIdentity identity, string recipientId)
        {
            //The recipient of the notification is allowed to modify it (e.g. mark is as read)
            return recipientId == identity.GetUserIdentity();
        }
    }

    [Flags]
    public enum UserRoles
    {
        None = 1,
        Administrator = 2,
        Supervisor = 4,
        Student = 8,
    }
}
