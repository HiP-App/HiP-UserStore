using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Entity;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Events;
using PaderbornUniversity.SILab.Hip.UserStore.Model.Rest;
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
            DateTimeOffset timestamp;

            while (await events.MoveNextAsync())
            {
                var currentEvent = events.Current;
                IEnumerable<PropertyChangedEvent> propEvents = new List<PropertyChangedEvent>();

                if (currentEvent is UserActivityBaseEvent)
                {
                    switch (currentEvent)
                    {
                        case UserCreated ev:
                            var emptyUserArgs = new UserArgs2();
                            e.AppendEvent(new CreatedEvent(ev.GetEntityType().Name, ev.Id, ev.UserId)
                            {
                                Timestamp = ev.Timestamp
                            });
                            var newUserArgs = new UserArgs2 
                            {
                                FirstName = ev.Properties?.FirstName,
                                LastName = ev.Properties?.LastName,
                                Email = ev.Properties?.Email
                            };
                            propEvents = EntityManager.CompareEntities(emptyUserArgs, newUserArgs, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUserArgs;
                            timestamp = ev.Timestamp;
                            break;

                        case UserUpdated ev:
                            timestamp = ev.Timestamp;
                            var currentArgs = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserArgs2;
                            if (currentArgs == null)
                                currentArgs = new UserArgs2();
                            newUserArgs = new UserArgs2
                            {
                                FirstName = ev.Properties?.FirstName,
                                LastName = ev.Properties?.LastName,
                                Email = currentArgs?.Email,
                                Password = currentArgs?.Password,
                                ProfilePicturePath = currentArgs?.ProfilePicturePath,
                                StudentDetails = currentArgs?.StudentDetails,
                                UserId = currentArgs?.UserId
                            };
                            propEvents = EntityManager.CompareEntities(currentArgs, newUserArgs, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUserArgs;
                            break;

                        case UserPhotoUploaded ev:
                            timestamp = ev.Timestamp;
                            var currentPhotoArgs = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserArgs2;
                            if (currentPhotoArgs == null)
                                currentPhotoArgs = new UserArgs2();
                            newUserArgs = new UserArgs2
                            {
                                ProfilePicturePath = ev?.Path,
                                FirstName = currentPhotoArgs?.FirstName,
                                LastName = currentPhotoArgs?.LastName,
                                Email = currentPhotoArgs?.Email,
                                Password = currentPhotoArgs?.Password,
                                StudentDetails = currentPhotoArgs?.StudentDetails,
                                UserId = currentPhotoArgs?.UserId
                            };
                            propEvents = EntityManager.CompareEntities(currentPhotoArgs, newUserArgs, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUserArgs;
                            break;

                        case UserStudentDetailsUpdated ev:
                            timestamp = ev.Timestamp;
                            var currentUserArgs = argumentDictionary[(ev.GetEntityType(), ev.Id)] as UserArgs2;
                            if (currentUserArgs == null)
                                currentUserArgs = new UserArgs2();
                            newUserArgs = new UserArgs2
                            {
                                StudentDetails = new StudentDetails(new StudentDetailsArgs
                                {
                                    CurrentDegree = ev.Properties?.CurrentDegree,
                                    CurrentSemester = ev.Properties.CurrentSemester,
                                    Discipline = ev.Properties?.Discipline
                                }),
                                FirstName = currentUserArgs?.FirstName,
                                LastName = currentUserArgs?.LastName,
                                Email = currentUserArgs?.Email,
                                Password = currentUserArgs?.Password,
                                ProfilePicturePath = currentUserArgs?.ProfilePicturePath,
                                UserId = currentUserArgs?.UserId
                            };
                            propEvents = EntityManager.CompareEntities(currentUserArgs, newUserArgs, 
                                ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = newUserArgs;
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
