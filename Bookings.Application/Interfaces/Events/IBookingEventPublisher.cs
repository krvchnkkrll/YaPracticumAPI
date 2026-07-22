using Kafka.Contracts.Events;

namespace Bookings.Application.Interfaces.Events;

public interface IBookingEventPublisher
{
    Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken);

    Task PublishBookingCancelledAsync(BookingCancelledEvent @event, CancellationToken cancellationToken);
}