using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class PredefinedAvatarsConfig
    {
        /// <summary>
        /// Path to the directory where profile pictures are stored.
        /// Default value: "Avatar"
        /// </summary>
        public string Path { get; set; } = "Avatar";

        /// <summary>
        /// A list of supported file extensions (without leading dot) for profile pictures.
        /// </summary>
        public List<string> SupportedFormats { get; set; }
    }
}
