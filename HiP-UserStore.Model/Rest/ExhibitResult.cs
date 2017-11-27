using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class ExhibitResult
    {
        public int Total { get; set; }

        public IList<Exhibit> Items { get; set; }
    }
}
