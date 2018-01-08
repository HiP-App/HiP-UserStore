using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.UserStore.Core
{
    /// <summary>
    /// Subscribes to EventStore events to keep the cache database up to date.
    /// </summary>
    public class CacheDatabaseManager
    {
        private readonly EventStoreService _eventStore;
        private readonly IMongoDatabase _db;

        public IMongoDatabase Database => _db;

        public CacheDatabaseManager(
            EventStoreService eventStore,
            IOptions<EndpointConfig> config,
            ILogger<CacheDatabaseManager> logger)
        {
            // For now, the cache database is always created from scratch by replaying all events.
            // This also implies that, for now, the cache database always contains the entire data (not a subset).
            // In order to receive all the events, a Catch-Up Subscription is created.

            // 1) Open MongoDB connection and clear existing database
            var mongo = new MongoClient(config.Value.MongoDbHost);
            mongo.DropDatabase(config.Value.MongoDbName);
            _db = mongo.GetDatabase(config.Value.MongoDbName);
            var uri = new Uri(config.Value.MongoDbHost);
            logger.LogInformation($"Connected to MongoDB cache database on '{uri.Host}', using database '{config.Value.MongoDbName}'");

            // 2) Subscribe to EventStore to receive all past and future events
            _eventStore = eventStore;
            _eventStore.EventStream.SubscribeCatchUp(ApplyEvent);
        }

        private void ApplyEvent(IEvent ev)
        {
            switch (ev)
            {
                case UserPhotoDeleted e:
                    var update3 = Builders<User>.Update
                        .Set(x => x.ProfilePicturePath, null)
                        .Set(x => x.Timestamp, e.Timestamp);

                    _db.GetCollection<User>(ResourceTypes.User.Name).UpdateOne(x => x.Id == e.Id, update3);
                    break;

                case CreatedEvent e:
                    var resourceType = e.GetEntityType();
                    switch (resourceType)
                    {
                        case ResourceType _ when resourceType == ResourceTypes.User:
                            var newUser = new User()
                            {
                                Id = e.Id,
                                UserId = e.UserId,
                                Timestamp = e.Timestamp
                            };
                            _db.GetCollection<User>(ResourceTypes.User.Name).InsertOne(newUser);
                            break;

                        case ResourceType _ when resourceType == ResourceTypes.StudentDetails:
                            var user = _db.GetCollection<User>(ResourceTypes.User.Name).AsQueryable().First(x => x.Id == e.Id);
                            var userArgs = user.CreateUserArgs2();
                            var updatedUser = new User(userArgs)
                            {
                                Id = e.Id,
                                UserId = e.UserId,
                                Timestamp = e.Timestamp,
                                StudentDetails = new StudentDetails()
                            };
                            _db.GetCollection<User>(ResourceTypes.User.Name).ReplaceOne(x => x.Id == e.Id, updatedUser);
                            break;
                    }
                    break;

                case PropertyChangedEvent e:
                    resourceType = e.GetEntityType();
                    switch (resourceType)
                    {
                        case ResourceType _ when resourceType == ResourceTypes.User:
                            var originalUser = _db.GetCollection<User>(ResourceTypes.User.Name).AsQueryable().First(x => x.Id == e.Id);
                            var userArgs = originalUser.CreateUserArgs2();
                            e.ApplyTo(userArgs);
                            var updatedUser = new User(userArgs)
                            {
                                Id = e.Id,
                                UserId = e.UserId,
                                Timestamp = e.Timestamp
                            };
                            _db.GetCollection<User>(ResourceTypes.User.Name).ReplaceOne(x => x.Id == e.Id, updatedUser);
                            break;

                        case ResourceType _ when resourceType == ResourceTypes.StudentDetails:
                            originalUser = _db.GetCollection<User>(ResourceTypes.User.Name).AsQueryable().First(x => x.Id == e.Id);
                            userArgs = originalUser.CreateUserArgs2();
                            var studentDetailsArgs = originalUser.StudentDetails.CreateStudentDetailsArgs();
                            e.ApplyTo(studentDetailsArgs);
                            updatedUser = new User(userArgs)
                            {
                                Id = e.Id,
                                UserId = e.UserId,
                                Timestamp = e.Timestamp,
                                StudentDetails = new StudentDetails(studentDetailsArgs)
                            };
                            _db.GetCollection<User>(ResourceTypes.User.Name).ReplaceOne(x => x.Id == e.Id, updatedUser);
                            break;
                    }
                    break;

                case DeletedEvent e:
                    resourceType = e.GetEntityType();
                    if (resourceType == ResourceTypes.StudentDetails)
                    {
                        var originalUser = _db.GetCollection<User>(ResourceTypes.User.Name).AsQueryable().First(x => x.Id == e.Id);
                        var userArgs = originalUser.CreateUserArgs2();
                        var updatedUser = new User(userArgs)
                        {
                            Id = e.Id,
                            UserId = e.UserId,
                            Timestamp = e.Timestamp,
                            StudentDetails = null
                        };
                        _db.GetCollection<User>(ResourceTypes.User.Name).ReplaceOne(x => x.Id == e.Id, updatedUser);
                    }
                    break;
            }
        }
    }
}
