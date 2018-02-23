using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{
    public abstract class BaseController<TArgs> : Controller
    {
        /// <summary>
        /// Determines whether the provided arguments are valid according to the specific requirements 
        /// </summary>
        /// <param name="args">Arguments to be validated</param>
        /// <returns>Validation result</returns>
        protected abstract Task<ArgsValidationResult> ValidateActionArgs(TArgs args);

        /// <summary>
        /// ResourceType that should be used to create new entities
        /// </summary>
        protected abstract ResourceType ResourceType { get; }
    }

    public class ArgsValidationResult
    {
        public bool Success { get; set; }
        public IActionResult ActionResult { get; set; }
    }

}
