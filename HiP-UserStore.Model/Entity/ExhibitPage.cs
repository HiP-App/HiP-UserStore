using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class ExhibitPage : ContentBase
    {
        public int ExhibitId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PageTypeDto Type { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public int? Audio { get; set; }

        public int? Image { get; set; }

        public IList<PageImageDto> Images { get; set; }

        public bool? HideYearNumbers { get; set; }

        public IList<int> AdditionalInformationPages { get; set; }

        public string Description { get; set; }

        public string FontFamily { get; set; }

    }

    public enum PageTypeDto
    {
        [EnumMember(Value = "Appetizer_Page")] AppetizerPage,
        [EnumMember(Value = "Image_Page")] ImagePage,
        [EnumMember(Value = "Slider_Page")] SliderPage,
        [EnumMember(Value = "Text_Page")] TextPage
    }

    public class PageImageDto
    {
        public long Date { get; set; }

        public int? Image { get; set; }
    }
}
