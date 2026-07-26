using Bookings.Application.Interfaces.Events;
using Bookings.Application.Interfaces.Identity;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Interfaces.Services;
using Bookings.Application.Models;
using Bookings.Domain.Entities.Bookings;
using Bookings.Domain.Entities.Bookings.Parameters;
using Bookings.Domain.Enums;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Static;
using Kafka.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Bookings.Application.Services;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    ICurrentUserService currentUserService,
    IBookingEventPublisher bookingEventPublisher,
    IAccessService accessService,
    ILogger<BookingService> logger) : IBookingService
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
    
    public async Task<CreateBookingResponse> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        var currentUserId = currentUserService.UserId;

        var activeUserBookings = await bookingRepository.GetCountUserActiveBookingsAsync(currentUserId, token);

        if (activeUserBookings >= BookingConstants.MaximumActiveUserBookings)
            throw new BookingLimitExceededException(BookingConstants.MaximumActiveUserBookings);
        
        try
        {
            await SemaphoreSlim.WaitAsync(token);

            var booking = Booking.Create(new CreateBookingParameters
            {
                EventId = eventId,
                UserId = currentUserId,
            });

            bookingRepository.Add(booking);

            await bookingRepository.SaveChangesAsync(token);

            return new CreateBookingResponse
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
            };
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<GetBookingResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        
        if (!accessService.IsCurrentUserHasAccessToBooking(booking))
            throw new BookingAccessDeniedException();
        
        if (ReferenceEquals(booking, null))
            throw new KeyNotFoundException($"Бронь с идентификатором {bookingId} не найдено.");
        
        return new GetBookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }

    public async Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (!accessService.IsCurrentUserHasAccessToBooking(booking))
            throw new BookingAccessDeniedException();

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.CancelledBooking();
        await bookingRepository.SaveChangesAsync(cancellationToken);

        if (!wasConfirmed)
            return;

        try
        {
            await bookingEventPublisher.PublishBookingCancelledAsync(new BookingCancelledEvent
            {
                BookingId = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                SeatsCount = 1,
                CancelledAt = booking.ProcessedAt!.Value,
            }, cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "Ошибка публикации брони {BookingId} в брокер сообщений.", bookingId);
        }
    }
}