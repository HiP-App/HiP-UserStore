using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class Exhibit : ContentBase
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool Used { get; set; }

        public IList<int> Tags { get; set; }

        public IList<int> Pages { get; set; }

        public int? Image { get; set; }
    }
}
