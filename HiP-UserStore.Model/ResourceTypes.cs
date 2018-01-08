using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.DataStore;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model
{
    public static class ResourceTypes
    {
        public static ResourceType User { get; private set; }
        public static ResourceType Exhibit { get; private set; }
        public static ResourceType ExhibitPage { get; private set; }
        public static ResourceType Route { get; private set; }
        public static ResourceType Tag { get; private set; }
        public static ResourceType Media { get; private set; }
        public static ResourceType StudentDetails { get; private set; }


        /// <summary>
        /// Initializes the fields
        /// </summary>
        public static void Initialize()
        {
            User = ResourceType.Register(nameof(User), typeof(UserArgs2));
            Exhibit = ResourceType.Register(nameof(Exhibit), typeof(ExhibitArgs));
            ExhibitPage = ResourceType.Register(nameof(ExhibitPage), typeof(ExhibitPageArgs2));
            Route = ResourceType.Register(nameof(Route), typeof(RouteArgs));
            Tag = ResourceType.Register(nameof(Tag), typeof(TagArgs));
            Media = ResourceType.Register(nameof(Media), typeof(MediaArgs));
            StudentDetails = ResourceType.Register(nameof(StudentDetails), typeof(StudentDetailsArgs));
        }
    }
}
