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
using System.Reflection;
using System;
using PaderbornUniversity.SILab.Hip.EventSourcing;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers
{

    [Authorize]
        [Route("api/[controller]")]
        public class ActionsController : Controller
        {
            private readonly CacheDatabaseManager _db;

            public ActionsController(CacheDatabaseManager db)
            {
                _db = db;
            }

            [HttpGet]
            [ProducesResponseType(typeof(AllItemsResult<ActionResult>), 200)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
        public IActionResult GetAllActions(string actionType = null)
            {
                  if (!ModelState.IsValid)
                   return BadRequest(ModelState);

                  if (actionType != null)
                  {
                      var listOfTypes =  typeof(ActionTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                       .Where(f => f.FieldType == typeof(ResourceType))
                                                       .ToDictionary(f => f.Name,
                                                                     f => (ResourceType)f.GetValue(null));
                
                      if (!listOfTypes.Any(x => x.Value.Name == actionType))
                          return NotFound(new { Message = $"Action type '{actionType}' is not supported" });
                  }

                var query = _db.Database.GetCollection<Action>(ResourceTypes.Action.Name).AsQueryable();
                var userId = User.Identity.GetUserIdentity();
                var result = query.Where(x => x.UserId == userId)
                                  .ToList()
                                  .AsQueryable()
                                  .FilterIf(actionType != null, x => x.TypeName == actionType)
                                  .Select(x => x.CreateActionResult())
                                  .ToList();
                return Ok(new AllItemsResult<ActionResult>() { Total = result.Count, Items = result });
            }         
    }    
}
