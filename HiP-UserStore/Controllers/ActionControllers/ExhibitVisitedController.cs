﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.Achievements.Model.Events;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest.Actions;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResourceType = PaderbornUniversity.SILab.Hip.UserStore.Model.ResourceType;

namespace PaderbornUniversity.SILab.Hip.UserStore.Controllers.ActionControllers
{
    public class ExhibitVisitedController : ActionBaseController<ExhibitVisitedActionArgs>
    {
        private readonly EndpointConfig _endpointConfig;
        private readonly ExhibitsVisitedIndex _index;

        public ExhibitVisitedController(EventStoreService eventStore, InMemoryCache cache, IOptions<EndpointConfig> endpointConfig) : base(eventStore, cache)
        {
            _endpointConfig = endpointConfig.Value;
            _index = cache.Index<ExhibitsVisitedIndex>();
        }


        [HttpPost("Many")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Post([FromBody] ExhibitVisitedActionsArgs args)
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

                var ev = new ActionCreated
                {
                    Id = _entityIndex.NextId(ResourceType.Action),
                    UserId = User.Identity.GetUserIdentity(),
                    Properties = arg,
                    Timestamp = DateTimeOffset.Now
                };

                await _eventStore.AppendEventAsync(ev);
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

        protected override async Task<ArgsValidationResult> ValidateActionArgs(ExhibitVisitedActionArgs args)
        {
            //check if the user has visited the exhibit already
            if (_index.Exists(User.Identity.GetUserIdentity(), args.EntityId))
            {
                return new ArgsValidationResult { ActionResult = BadRequest(new { Message = "The user has already visited this exhibit" }) };
            }

            //check if exhibits exists
            var client = new ExhibitsClient(_endpointConfig.DataStoreUrl)
            {
                Authorization = Request.Headers["Authorization"]
            };

            try
            {
                //this method throws a SwaggerException if the request fails 
                await client.GetByIdAsync(args.EntityId, null);
                return new ArgsValidationResult { Success = true };
            }
            catch (SwaggerException)
            {
                return new ArgsValidationResult { ActionResult = NotFound(new { Message = "An exhibit with this id doesn't exist" }), Success = false };
            }
        }
    }
}
