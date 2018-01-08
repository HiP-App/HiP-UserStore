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

                if (currentEvent is UserActivityBaseEvent userEvent)
                {
                    switch (currentEvent)
                    {
                        case UserCreated ev:
                            var emptyUserArgs = new UserArgs();
                            e.AppendEvent(new CreatedEvent(ev.GetEntityType().Name, ev.Id, ev.UserId)
                            {
                                Timestamp = ev.Timestamp
                            });
                            propEvents = EntityManager.CompareEntities(emptyUserArgs, ev.Properties, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = ev.Properties;
                            timestamp = ev.Timestamp;
                            break;

                        case UserUpdated ev:
                            timestamp = ev.Timestamp;
                            var newArgs = ev.Properties;
                            var currentArgs = (UserUpdateArgs)argumentDictionary[(ev.GetEntityType(), ev.Id)];
                            propEvents = EntityManager.CompareEntities(currentArgs, newArgs, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = ev.Properties;
                            break;

                        case UserPhotoUploaded ev:
                            timestamp = ev.Timestamp;
                            var newPhotoArgs = new UserArgs2
                            {
                                ProfilePicturePath = ev.Path
                            };
                            // TODO: test if string of path is enough to save in dictionary or if UserArgs2 need to be stored.
                            var currentPhotoArgs = (UserArgs2)argumentDictionary[(ev.GetEntityType(), ev.Id)];
                            propEvents = EntityManager.CompareEntities(currentPhotoArgs, newPhotoArgs, ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = ev.Path;
                            break;

                        case UserStudentDetailsUpdated ev:
                            timestamp = ev.Timestamp;
                            var newStudentDetailArgs = new StudentDetailsArgs
                            {
                                CurrentDegree = ev.Properties.CurrentDegree,
                                CurrentSemester = ev.Properties.CurrentSemester,
                                Discipline = ev.Properties.Discipline
                            };
                            var currentStudentDetailsArgs = (StudentDetailsArgs)argumentDictionary[(ev.GetEntityType(), ev.Id)];
                            propEvents = EntityManager.CompareEntities(currentStudentDetailsArgs, newStudentDetailArgs, 
                                ev.GetEntityType(), ev.Id, ev.UserId);
                            argumentDictionary[(ev.GetEntityType(), ev.Id)] = ev.Properties;
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
