using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
using PaderbornUniversity.SILab.Hip.UserStore.Model.EventArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Migrations
{
    [StreamMigration(from: 0, to: 1)]
    public class Migration1PropertyChanged : IStreamMigration
    {
        private Dictionary<(ResourceType, int), object> argumentDictionary = new Dictionary<(ResourceType, int), object>();

        public async Task MigrateAsync(IStreamMigrationArgs e)
        {
            var events = e.GetExistingEvents();
            DateTimeOffset timestamp=new DateTimeOffset();

            while (await events.MoveNextAsync())
            {
                var currentEvent = events.Current;
                IEnumerable<PropertyChangedEvent> propEvents = new List<PropertyChangedEvent>();

                if (currentEvent is UserActivityBaseEvent)
                {
                    switch (currentEvent)
                    {
                        case UserCreated ev:
                            var emptyUser = new UserEventArgs();
                            e.AppendEvent(new CreatedEvent(ev.GetEntityType().Name, ev.Id, ev.UserId)
                            {
                                Timestamp = ev.Timestamp
                            });
                            var newUser = new UserEventArgs
                            {
                                FirstName = ev.Properties?.FirstName,
                                LastName = ev.Properties?.LastName,
                                Email = ev.Properties?.Email,
                                UserId = ev.UserId
                            };
                            propEvents = EntityManager.CompareEntities(emptyUser, newUser, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUser;
                            timestamp = ev.Timestamp;
                            break;

                        case UserUpdated ev:
                            timestamp = ev.Timestamp;
                            var currentUser = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserEventArgs;
                            if (currentUser == null)
                                currentUser = new UserEventArgs();
                            newUser = new UserEventArgs(currentUser)
                            {
                                FirstName = ev.Properties?.FirstName,
                                LastName = ev.Properties?.LastName
                            };
                            propEvents = EntityManager.CompareEntities(currentUser, newUser, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUser;
                            break;

                        case UserPhotoUploaded ev:
                            timestamp = ev.Timestamp;
                            currentUser = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserEventArgs;
                            if (currentUser == null)
                                currentUser = new UserEventArgs();
                            newUser = new UserEventArgs(currentUser)
                            {
                                ProfilePicturePath = ev.Path
                            };
                            propEvents = EntityManager.CompareEntities(currentUser, newUser, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUser;
                            break;

                        case UserPhotoDeleted ev:
                            timestamp = ev.Timestamp;
                            currentUser = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserEventArgs;
                            if (currentUser == null)
                                currentUser = new UserEventArgs();
                            newUser = new UserEventArgs(currentUser)
                            {
                                ProfilePicturePath = null
                            };
                            propEvents = EntityManager.CompareEntities(currentUser, newUser, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUser;
                            break;

                        case UserStudentDetailsUpdated ev:
                            timestamp = ev.Timestamp;
                            currentUser = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserEventArgs;
                            if (currentUser == null)
                                currentUser = new UserEventArgs();
                            newUser = new UserEventArgs(currentUser)
                            {
                                StudentDetails = new StudentDetails(new StudentDetailsArgs
                                {
                                    CurrentDegree = ev.Properties?.CurrentDegree,
                                    CurrentSemester = ev.Properties?.CurrentSemester,
                                    Discipline = ev.Properties?.Discipline
                                })
                            };
                            propEvents = EntityManager.CompareEntities(currentUser, newUser, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUser;
                            break;

                        default:
                            //append all other events
                            e.AppendEvent(events.Current);
                            break;
                    }

                    foreach (var propEvent in propEvents)
                    {
                        propEvent.Timestamp = timestamp;
                        e.AppendEvent(propEvent);
                    }
                }
                else
                {
                    e.AppendEvent(events.Current);
                }
            }
        }
    }
}
