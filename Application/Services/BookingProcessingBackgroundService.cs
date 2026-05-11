using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingProcessingBackgroundService(IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var pendingBooks = bookingRepository.GetPendingBooks();
            foreach (var pendingBook in pendingBooks)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                pendingBook.ConfirmBooking();
            }
        }
    }
}