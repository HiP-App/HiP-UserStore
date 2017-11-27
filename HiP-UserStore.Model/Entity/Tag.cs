using System;
using System.Collections.Generic;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class Tag : ContentBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int? Image { get; set; }

        public bool Used { get; set; }
    }
}
