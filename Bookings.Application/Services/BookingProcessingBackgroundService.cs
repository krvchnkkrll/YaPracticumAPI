using Bookings.Application.Interfaces.Events;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Domain.Enums;
using Kafka.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Application.Services;

internal sealed class BookingProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IBookingEventPublisher bookingEventPublisher,
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

                var pendingBookings =
                    await bookingRepository.GetBookingsByStatusesAsync([BookingStatus.Pending], stoppingToken);

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));

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

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        await _processingSemaphore.WaitAsync(stoppingToken);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var bookingRepository =
                scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            
            var booking = await bookingRepository.GetByIdOrDefaultAsync(bookingId, stoppingToken);

            if (booking is null)
            {
                logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            booking.ConfirmBooking();

            await bookingRepository.SaveChangesAsync(stoppingToken);

            try
            {
                await bookingEventPublisher.PublishBookingConfirmedAsync(new BookingConfirmedEvent
                {
                    BookingId = booking.Id,
                    EventId = booking.EventId,
                    UserId = booking.UserId,
                    SeatsCount = 1,
                    ConfirmedAt = booking.ProcessedAt!.Value,
                }, stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(
                    e,
                    "Бронь {BookingId} подтверждена в БД, но не удалось опубликовать событие в брокер сообщений",
                    bookingId);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Неожиданная ошибка при обработке брони {BookingId}", bookingId);

            await RejectBookingAfterErrorAsync(bookingId, stoppingToken);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
    
    private async Task RejectBookingAfterErrorAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var bookingRepository =
                scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var booking = await bookingRepository.GetByIdOrDefaultAsync(bookingId, stoppingToken);

            if (booking is null)
                return;

            booking.RejectBooking();

            await bookingRepository.SaveChangesAsync(stoppingToken);

            logger.LogWarning("Бронь {BookingId} отклонена после ошибки", bookingId);
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(rollbackEx, "Ошибка при отклонении брони {BookingId} после сбоя", bookingId);
        }
    }
}