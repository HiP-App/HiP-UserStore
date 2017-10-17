using System;
using System.Linq;
using System.Security.Principal;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class UserPermissions
    {
        public static bool IsAllowedToGetPhoto(IIdentity identity, string ownerId)
        {
            // Profile pictures are public (for now) - anyone can access anyone else's photo
            return true;
        }

        public static bool IsAllowedToChangePhoto(IIdentity identity, string ownerId)
        {
            // Users can change their own profile picture, admins/supervisors can change picture of any user
            return (identity.GetUserIdentity() == ownerId) || CheckRoles(identity);
        }

        public static bool IsAllowedToGet(IIdentity identity, string requestedUserId)
        {
            // Users can get info about themselves, admins/supervisors can get info of any user
            return (identity.GetUserIdentity() == requestedUserId) || CheckRoles(identity);
        }

        public static bool IsAllowedToGetAll(IIdentity identity) => CheckRoles(identity);
        
        // Check if the user has the nessesary roles
        static bool CheckRoles(IIdentity identity, UserRoles allowedToProceed = UserRoles.Administrator | UserRoles.Supervisor)
        {
            return identity.GetUserRoles()
                           .Any(x => (Enum.TryParse(x.Value, out UserRoles role) && (allowedToProceed & role) != 0)); // Bitwise AND
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
