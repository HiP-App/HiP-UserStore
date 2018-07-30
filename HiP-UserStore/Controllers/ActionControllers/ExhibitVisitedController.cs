using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using Action = PaderbornUniversity.SILab.Hip.UserStore.Model.Entity.Action;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers.ActionControllers
{
    public class ExhibitVisitedController : ActionBaseController<ExhibitVisitedActionArgs>
    {
        private readonly ExhibitsVisitedIndex _index;
        private readonly DataStoreService _dataStoreService;
        private readonly CacheDatabaseManager _db;

        public ExhibitVisitedController(EventStoreService eventStore, InMemoryCache cache, DataStoreService dataStoreService, CacheDatabaseManager db) : base(eventStore, cache)
        {
            _index = cache.Index<ExhibitsVisitedIndex>();
            _dataStoreService = dataStoreService;
            _db = db;
        }

        protected override ResourceType ResourceType => ActionTypes.ExhibitVisited;

        /// <summary>
        /// Posts multiple ExhibitVisistedActions
        /// </summary>
        [HttpPost("Many")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PostMany([FromBody] ExhibitVisitedActionsArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var argList = args.ToListActionArgs();
            var validationResultList = new List<(int, ArgsValidationResult)>();

            foreach (var arg in argList)
            {
                validationResultList.Add((arg.EntityId, await ValidateActionArgs((ExhibitVisitedActionArgs)arg)));
                if (!validationResultList.Last().Item2.Success)
                {
                    continue;
                }

                var id = _entityIndex.NextId(ResourceType);
                await EntityManager.CreateEntityAsync(_eventStore, (ExhibitVisitedActionArgs)arg, ResourceType, id, User.Identity.GetUserIdentity());
            }
            if (validationResultList.Any(x => x.Item2.Success))
            {
                return StatusCode(201, String.Join(',', validationResultList.FindAll(x => x.Item2.Success)
                                                                            .Select(x => x.Item1)
                                                                            .ToArray()));
            }
            else
            {
                return StatusCode(400, String.Join(',', validationResultList.Select(x => x.Item1).ToArray()));
            }
        }

        /// <summary>
        /// Get all Actions of Exhibit Visited type of all users by exhibit ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("All/{exhibitId}")]
        [ProducesResponseType(typeof(AllItemsResult<ActionResultBase>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAll(int exhibitId, DateTimeOffset? timestamp = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exhibitExist = await ExhibitExists(exhibitId);
            if (!exhibitExist.Success)
                return StatusCode(400, exhibitExist.ActionResult);

            if (!UserPermissions.IsAllowedToGetAllActions(User.Identity) || User.Identity.GetUserIdentity() == null)
                return Forbid();

            var query = _db.Database.GetCollection<Action>(ResourceTypes.Action.Name).AsQueryable();

            var result = query.Where(x => (x.EntityId == exhibitId))
                              .FilterByTimestamp(timestamp).ToList()
                              .Where(x => (x.TypeName == ActionTypes.ExhibitVisited.Name))
                              .Select(x => (ExhibitVisitedActionResult)x.CreateActionResult())
                              .ToList();
            return Ok(new AllItemsResult<ActionResultBase>() { Total = result.Count, Items = result });
        }

        protected override async Task<ArgsValidationResult> ValidateActionArgs(ExhibitVisitedActionArgs args)
        {
            //check if the user has visited the exhibit already
            if (_index.Exists(User.Identity.GetUserIdentity(), args.EntityId))
            {
                return new ArgsValidationResult { ActionResult = BadRequest(new { Message = "The user has already visited this exhibit" }) };
            }

            return await ExhibitExists(args.EntityId);
        }

        /// <summary>
        /// check if exhibits exists. Quering DataStore Service for result.
        /// </summary>
        /// <param name="id">Id of exhibit</param>
        /// <returns></returns>
        private async Task<ArgsValidationResult> ExhibitExists(int id)
        {
            try
            {
                //this method throws a SwaggerException if the request fails 
                await _dataStoreService.Exhibits.GetByIdAsync(id, null);
                return new ArgsValidationResult { Success = true };
            }
            catch (SwaggerException)
            {
                return new ArgsValidationResult { ActionResult = NotFound(new { Message = "An exhibit with this id doesn't exist" }), Success = false };
            }
        }
    }
}
