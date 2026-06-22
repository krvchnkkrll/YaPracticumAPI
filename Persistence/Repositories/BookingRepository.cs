using Domain.Entities.Bookings;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contracts;
using Persistence.Contracts.Repositories;

namespace Persistence.Repositories;

public sealed class BookingRepository(IDbContext context) : IBookingRepository
{
    public async Task<Booking> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await GetByIdOrDefaultAsync(bookingId, cancellationToken);
        
        if (ReferenceEquals(booking, null))
            throw new KeyNotFoundException($"Бронь с идентификатором {bookingId} не найдено.");
        
        return booking;
    }

    private async Task<Booking?> GetByIdOrDefaultAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return await context.Bookings.SingleOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }
    
    public async Task<IReadOnlyList<Booking>> GetBookingsByStatusesAsync(BookingStatus[] statuses, CancellationToken cancellationToken)
    {
        return await context.Bookings
            .Where(b => statuses.Contains(b.Status))
            .ToArrayAsync(cancellationToken);
    }
    
    public void Remove(IEnumerable<Booking> bookings)
    {
        context.Bookings.RemoveRange(bookings);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}