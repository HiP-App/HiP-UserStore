using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.UserStore.Migrations
{
    [StreamMigration(from: 1, to: 2)]
    public class Migration2FixReportedActionIds : IStreamMigration
    {
        public async Task MigrateAsync(IStreamMigrationArgs e)
        {
            var events = e.GetExistingEvents();
            int lastIdSeen = 0;
            int nextId = 0;
            while (await events.MoveNextAsync())
            {
                switch (events.Current)
                {
                    case CreatedEvent ev when ev.GetEntityType() == ActionTypes.ExhibitVisited:
                        if (ev.Id <= lastIdSeen)
                        {
                            ev.Id = nextId;
                        }
                        lastIdSeen = ev.Id;
                        nextId++;
                        e.AppendEvent(ev);
                        break;

                    case PropertyChangedEvent ev when ev.GetEntityType() == ActionTypes.ExhibitVisited:
                        if (ev.Id <= lastIdSeen)
                        {
                            ev.Id = lastIdSeen;
                        }
                        e.AppendEvent(ev);
                        break;

                    default:
                        e.AppendEvent(events.Current);
                        break;
                }
            }
        }
    }
}
