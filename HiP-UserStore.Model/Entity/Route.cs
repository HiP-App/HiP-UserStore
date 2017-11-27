using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class Route : ContentBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int Duration { get; set; }

        public double Distance { get; set; }

        public int? Image { get; set; }

        public int? Audio { get; set; }

        public IList<int> Exhibits { get; set; }

        public IList<int> Tags { get; set; }
    }
}
