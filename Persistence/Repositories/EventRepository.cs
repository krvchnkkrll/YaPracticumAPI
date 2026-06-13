using Application.Contracts.Models;
using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Domain.Models.Pagination;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;

namespace Persistence.Repositories;

internal sealed class EventRepository(IEventStorage eventStorage) : IEventRepository
{
    /// <summary>
    ///     Получить событие по id
    /// </summary>
    public Event GetEventById(Guid eventId)
    {
        var eventToReturn = GetEventOrDefaultById(eventId);
        
        if (ReferenceEquals(eventToReturn, null))
            throw new KeyNotFoundException($"Событие с идентификатором {eventId} не найдено.");
        
        return eventToReturn;
    }

    public Event? GetEventOrDefaultById(Guid eventId)
    {
        var eventToReturn = eventStorage.Events.SingleOrDefault(e => e.Id == eventId);
        
        return eventToReturn;
    }

    /// <summary>
    ///     Получить все события
    /// </summary>
    public PaginatedResult<Event> GetPaginatedEvents(GetEventsSearchQuery searchQuery, PaginationQuery paginationQuery)
    {
        var query = eventStorage.Events.AsQueryable();

        #region  Filters

        if (!string.IsNullOrEmpty(searchQuery.Title))
        {
            query = query.Where(e => e.Title.Contains(searchQuery.Title, StringComparison.InvariantCultureIgnoreCase));
        }
        if (searchQuery.From.HasValue)
        {
            query = query.Where(e => e.StartAt >= searchQuery.From.Value);
        }
        if (searchQuery.To.HasValue)
        {
            query = query.Where(e => e.EndAt <= searchQuery.To.Value);
        }

        #endregion

        #region Pagination
        
        var totalItems = query.Count();
        
        var totalPages = totalItems == 0 
            ? 0 
            : (int)Math.Ceiling(totalItems / (double) paginationQuery.PageSize);
        
        var skip = (paginationQuery.Page - 1) * paginationQuery.PageSize;
        
        var items = query
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(paginationQuery.PageSize)
            .ToArray();
        
        #endregion
        
        return new PaginatedResult<Event>
        {
            Items = items,
            TotalItems = totalItems,
            CurrentPage = paginationQuery.Page,
            PageSize = paginationQuery.PageSize,
            TotalPages = totalPages
        };
    }

    /// <summary>
    ///     Получить все события
    /// </summary>
    /// <returns></returns>
    public IList<Event> GetAllEvents()
    {
        return eventStorage.Events.ToList();
    }

    /// <summary>
    ///     Добавить событие
    /// </summary>
    public Event Add(CreateEventParameter parameter)
    {
        var newEvent = Event.Create(parameter);
        
        eventStorage.Events.Add(newEvent);
        
        return newEvent;
    }

    /// <summary>
    ///     Обновить событие
    /// </summary>
    public void Update(Guid eventId, UpdateEventParameter parameter)
    {
        var eventToUpdate = GetEventById(eventId);
        
        eventToUpdate.Update(parameter);
    }

    /// <summary>
    ///     Удалить событие
    /// </summary>
    public void Delete(Guid eventId)
    {
        var eventToRemove = GetEventById(eventId);

        eventStorage.Events.Remove(eventToRemove);
    }
}