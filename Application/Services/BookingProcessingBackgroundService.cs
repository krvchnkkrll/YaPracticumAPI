using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Contracts;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IDbContextFactory contextFactory,
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
            await using var context = await contextFactory.CreateDbContextAsync(stoppingToken);

            var booking = await context.Bookings
                .Include(b => b.Event)
                .SingleOrDefaultAsync(b => b.Id == bookingId, stoppingToken);

            if (booking is null)
            {
                logger.LogWarning("Бронь {BookingId} не найдена", bookingId);
                return;
            }

            if (booking.Event is null)
            {
                booking.RejectBooking();

                logger.LogWarning(
                    "Бронь {BookingId} отклонена: событие {EventId} не найдено",
                    booking.Id,
                    booking.EventId);

                await context.SaveChangesAsync(stoppingToken);
                return;
            }

            booking.ConfirmBooking();

            await context.SaveChangesAsync(stoppingToken);
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
            await using var context = await contextFactory.CreateDbContextAsync(stoppingToken);

            var booking = await context.Bookings
                .Include(b => b.Event)
                .SingleOrDefaultAsync(b => b.Id == bookingId, stoppingToken);

            if (booking is null)
                return;

            booking.RejectBooking();

            if (booking.Event is not null)
                booking.Event.ReleaseSeats();

            await context.SaveChangesAsync(stoppingToken);

            logger.LogWarning("Бронь {BookingId} отклонена после ошибки", bookingId);
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(rollbackEx, "Ошибка при отклонении брони {BookingId} после сбоя", bookingId);
        }
    }
}