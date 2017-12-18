using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.UserStore.Utility
{
    public class UploadPhotoConfig
    {
        /// <summary>
        /// Path to the directory where profile pictures are stored.
        /// Default value: "Photo"
        /// </summary>
        public string Path { get; set; } = "Photo";

        /// <summary>
        /// A list of supported file extensions (without leading dot) for profile pictures.
        /// </summary>
        public List<string> SupportedFormats { get; set; }
    }
}
