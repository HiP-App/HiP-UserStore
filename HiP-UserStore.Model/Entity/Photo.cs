namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public class Photo : ContentBase
    {
        /// <summary>
        /// Can be updated by admin or supervisor
        /// </summary>
        public string LastUpdatedBy { get; set; }
        /// <summary>
        /// The path to the actual file.
        /// </summary>
        public string Path { get; set; }
    }
}
