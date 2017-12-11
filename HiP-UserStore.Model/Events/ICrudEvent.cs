using PaderbornUniversity.SILab.Hip.EventSourcing;
using System;

namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Events
{
    /// <summary>
    /// An event for simple create, update and delete operations.
    /// </summary>
    public interface ICrudEvent : IEvent
    {
        /// <summary>
        /// Gets the type of the created, updated or deleted entity.
        /// </summary>
        ResourceType GetEntityType();

        /// <summary>
        /// The ID of the created, updated or deleted entity.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The date and time when the entity was created, updated or deleted.
        /// </summary>
        DateTimeOffset Timestamp { get; }
    }
    public interface ICreateEvent : ICrudEvent
    {
    }

    public interface IUpdateEvent : ICrudEvent
    {
    }

    public interface IDeleteEvent : ICrudEvent
    {
    }

    public interface IUserActivityEvent : ICrudEvent
    {
        /// <summary>
        /// The ID of the user that initiated the event.
        /// In creation-events, this is also known as the "entity owner".
        /// </summary>
        /// <remarks>
        /// In <see cref="UserCreated"/>-events, this is actually the ID of the created user (in other words:
        /// each user is its own owner). In other user-related events like <see cref="UserUpdated"/>, remember this
        /// does NOT necessarily refer to the updated user, but rather to the user "who called the PUT-API" to
        /// update himself or some other user.
        /// </remarks>
        string UserId { get; }
    }
}
