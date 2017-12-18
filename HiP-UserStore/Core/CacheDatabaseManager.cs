using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using System;
using ResourceType = PaderbornUniversity.SILab.Hip.UserStore.Model.ResourceType; // TODO: Remove after architectural changes

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
                case UserCreated e:
                    var newUser = new User
                    {
                        Id = e.Id,
                        UserId = e.UserId,
                        Timestamp = e.Timestamp,
                        Email = e.Properties?.Email,
                        FirstName = e.Properties?.FirstName,
                        LastName = e.Properties?.LastName
                    };
                    _db.GetCollection<User>(ResourceType.User.Name).InsertOne(newUser);
                    break;

                case UserUpdated e:
                    var update = Builders<User>.Update
                        .Set(x => x.FirstName, e.Properties.FirstName)
                        .Set(x => x.LastName, e.Properties.LastName);

                    _db.GetCollection<User>(ResourceType.User.Name).UpdateOne(x => x.Id == e.Id, update);
                    break;

                case UserPhotoUploaded e:
                    var update2 = Builders<User>.Update
                        .Set(x => x.ProfilePicturePath, e.Path)
                        .Set(x => x.Timestamp, e.Timestamp);

                    _db.GetCollection<User>(ResourceType.User.Name).UpdateOne(x => x.Id == e.Id, update2);
                    break;

                case UserPhotoDeleted e:
                    var update3 = Builders<User>.Update
                        .Set(x => x.ProfilePicturePath, null)
                        .Set(x => x.Timestamp, e.Timestamp);

                    _db.GetCollection<User>(ResourceType.User.Name).UpdateOne(x => x.Id == e.Id, update3);
                    break;

                case UserStudentDetailsUpdated e:
                    var studentDetails = e.Properties == null ? null : new StudentDetails(e.Properties);
                    var update4 = Builders<User>.Update.Set(x => x.StudentDetails, studentDetails);
                    _db.GetCollection<User>(ResourceType.User.Name).UpdateOne(x => x.Id == e.Id, update4);
                    break;
            }

        }
    }
}
