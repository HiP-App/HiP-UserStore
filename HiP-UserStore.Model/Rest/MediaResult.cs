using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class MediaResult
    {
        public int Total { get; set; }

        public IList<Media> Items { get; set; }
    }
}
