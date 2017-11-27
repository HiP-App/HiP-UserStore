﻿using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class ExhibitPageResult
    {
        public int Total { get; set; }

        public IList<ExhibitPage> Items { get; set; }
    }
}