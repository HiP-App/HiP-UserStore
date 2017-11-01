using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Rest
{
    public class QueryArgs
    {
        /// <summary>
        /// The page number. Defaults to one, which is the first page.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        /// <summary>
        /// Maximum number of entities in the response. The last page may have less entities,
        /// all other pages have exactly <see cref="PageSize"/> entities.
        /// </summary>
        /// <remarks>
        /// If <see cref="Page"/> is specified (!= null), the page size defaults to 10.
        /// Otherwise, it defaults to <see cref="int.MaxValue"/>, so that all items are returned.
        /// </remarks>
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }

        /// <summary>
        /// The field to order the response data by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Restricts the reponse results to the objects containing the queried string in a text field
        /// (title/name, description).
        /// </summary>
        public string Query { get; set; }
    }
}
