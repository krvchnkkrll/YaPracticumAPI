using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingProcessingBackgroundService> logger)
    : BackgroundService
{
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

                var pendingBooks = bookingRepository.GetPendingBooks();

                foreach (var pendingBook in pendingBooks)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                        pendingBook.ConfirmBooking();
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ошибка подтверждения брони {BookingId}", pendingBook.Id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"{nameof(BookingProcessingBackgroundService)} остановлен");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Ошибка {nameof(BookingProcessingBackgroundService)}");
            }
        }
    }
}