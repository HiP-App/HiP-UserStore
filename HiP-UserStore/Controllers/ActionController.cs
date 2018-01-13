using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Action = PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Action;
using ActionResult = PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.ActionResult;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{

    [Authorize]
        [Route("api/[controller]")]
        public class ActionController : Controller
        {
            private readonly CacheDatabaseManager _db;

            public ActionController(CacheDatabaseManager db)
            {
                _db = db;
            }

            [HttpGet("all")]
            [ProducesResponseType(typeof(AllItemsResult<ActionResult>), 200)]
            public IActionResult GetAllActions()
            {
                var query = _db.Database.GetCollection<Action>(ResourceType.Action.Name).AsQueryable();
                var userId = User.Identity.GetUserIdentity();
                var result = query.Where(x => x.UserId == userId).ToList()
                    .Select(x => x.CreateActionResult())
                    .ToList();
                return Ok(new AllItemsResult<ActionResult>() { Total = result.Count, Items = result });
            }
        }
    
}
