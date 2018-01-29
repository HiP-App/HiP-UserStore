using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public static class ResourceTypes
    {
        public static ResourceType User;
        public static ResourceType Exhibits;
        public static ResourceType Routes;
        public static ResourceType Media;
        public static ResourceType Tags;
        public static ResourceType ExhibitPages;
        public static ResourceType Action;

        /// <summary>
        /// Initializes the fieldd
        /// </summary>
        public static void Initialize()
        {
            User = ResourceType.Register(nameof(User), typeof(UserArgs));
            Exhibits = ResourceType.Register(nameof(Exhibits), null);
            Routes = ResourceType.Register(nameof(Routes), null);
            Media = ResourceType.Register(nameof(Media), null);
            ExhibitPages = ResourceType.Register(nameof(ExhibitPages), null);
            Tags = ResourceType.Register(nameof(Tags), null);
            Action = ResourceType.Register(nameof(Action), typeof(ActionArgs));
        }
    }
}

