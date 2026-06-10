using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;

namespace Application.Services;

internal sealed class BookingProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IEventStorage eventStorage,
    ILogger<BookingProcessingBackgroundService> logger)
    : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);

                await using var scope = scopeFactory.CreateAsyncScope();

                var bookingRepository =
                    scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var pendingBookings = bookingRepository
                    .GetPendingBooks()
                    .ToArray();

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{ServiceName} остановлен", nameof(BookingProcessingBackgroundService));

                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка {ServiceName}", nameof(BookingProcessingBackgroundService));
            }
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        await _processingSemaphore.WaitAsync(stoppingToken);
        Event? eventEntity = null;

        try
        {
            eventEntity = eventStorage.Events.SingleOrDefault(e => e.Id == booking.EventId);

            if (eventEntity is null)
            {
                booking.RejectBooking();
                logger.LogWarning("Бронь {BookingId} отклонена: событие {EventId} не найдено", booking.Id,
                    booking.EventId);
                return;
            }

            booking.ConfirmBooking();
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Неожиданная ошибка при обработке брони {BookingId}", booking.Id);

            try
            {
                booking.RejectBooking();

                if (eventEntity is not null)
                {
                    eventEntity.ReleaseSeats();
                }

                logger.LogWarning("Бронь {BookingId} отклонена после ошибки", booking.Id);
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Ошибка при отклонении брони {BookingId} после сбоя", booking.Id);
            }
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}