using Kafka.Contracts.Events;

namespace Bookings.Application.Interfaces.Events;

public interface IBookingConfirmedPublisher
{
    Task PublishAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken);
}