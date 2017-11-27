using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class TagResult
    {
        public int Total { get; set; }

        public IList<Tag> Items { get; set; }
    }
}
