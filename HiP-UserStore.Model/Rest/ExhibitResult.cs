using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class ExhibitResult
    {
        public int Total { get; set; }

        public IList<Exhibit> Items { get; set; }
    }
}
