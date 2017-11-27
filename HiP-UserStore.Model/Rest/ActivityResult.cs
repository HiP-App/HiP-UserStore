using System.Collections.Generic;


namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class ActivityResult
    {
        public List<int> ExhibitIds { get; set; } = new List<int>();

        public List<int> RouteIds { get; set; } = new List<int>();

        public List<int> MediaIds { get; set; } = new List<int>();

        public List<int> TagIds { get; set; } = new List<int>();

        public List<int> ExhibitPageIds { get; set; } = new List<int>();
    }
}
