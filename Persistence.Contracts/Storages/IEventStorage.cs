using Domain.Entities.Events;

namespace Persistence.Contracts.Storages;

public interface IEventStorage
{
    /// <summary>
    ///     Получить заложенные в памяти мероприятия
    /// </summary>
    List<Event> Events { get; }
}