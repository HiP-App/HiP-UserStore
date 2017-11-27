using System;
using System.Collections.Generic;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class Media : ContentBase
    {

        public string Title { get; set; }

        public string Description { get; set; }

        public MediaTypeDto Type { get; set; }

        public bool Used { get; set; }
    }

    public enum MediaTypeDto
    {
        Audio,
        Image
    }
}
