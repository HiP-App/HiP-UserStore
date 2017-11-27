using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class RouteResult
    {
        public int Total { get; set; }

        public IList<Route> Items { get; set; }
    }
}
