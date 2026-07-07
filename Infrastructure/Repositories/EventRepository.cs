using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Domain.Models.Pagination;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class EventRepository(IDbContext context) : IEventRepository
{
    /// <summary>
    ///     Получить событие по id
    /// </summary>
    public async Task<Event> GetReadOnlyByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventToReturn = await GetByIdOrDefaultReadOnlyAsync(eventId, cancellationToken);

        if (ReferenceEquals(eventToReturn, null))
            throw new KeyNotFoundException($"Событие с идентификатором {eventId} не найдено.");

        return eventToReturn;
    }

    public async Task<PaginatedResult<Event>> GetPaginatedAsync(GetEventsSearchQuery searchQuery,
        PaginationQuery paginationQuery,
        CancellationToken cancellationToken)
    {
        var query = context.Events.AsQueryable();

        #region Filters

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

        var totalItems = await query.CountAsync(cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)paginationQuery.PageSize);

        var skip = (paginationQuery.Page - 1) * paginationQuery.PageSize;

        var items = await query
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(paginationQuery.PageSize)
            .ToArrayAsync(cancellationToken);

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

    public async Task<IReadOnlyList<Event>> GetAllReadOnlyAsync(CancellationToken cancellationToken)
    {
        return await context.Events.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Events.ToListAsync(cancellationToken);
    }

    public Booking CreateBooking(Event eventEntity)
    {
        return eventEntity.CreateBooking();
    }

    public void Add(Event eventEntity)
    {
        context.Events.Add(eventEntity);
    }

    public void Remove(Event eventEntity)
    {
        context.Events.Remove(eventEntity);
    }

    public async Task<Event> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventToReturn = await GetByIdOrDefaultAsync(eventId, cancellationToken);

        if (ReferenceEquals(eventToReturn, null))
            throw new KeyNotFoundException($"Событие с идентификатором {eventId} не найдено.");

        return eventToReturn;
    }

    private async Task<Event?> GetByIdOrDefaultAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await context.Events.SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    private async Task<Event?> GetByIdOrDefaultReadOnlyAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await context.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    public async Task<Event> GetByIdWithIncludeBookingsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await context.Events
                   .Include(e => e.Bookings)
                   .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken) ??
               throw new KeyNotFoundException($"Событие с идентификатором {eventId} не найдено.");
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}