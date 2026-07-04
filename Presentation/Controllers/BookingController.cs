using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Route("api/bookings")]
public sealed class BookingController(IBookingService bookingService) : AppController
{
    /// <summary>
    ///     Получить бронь
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetBookingByIdAsync))]
    public async Task<ActionResult<GetBookingResponse>> GetBookingByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await bookingService.GetBookingByIdAsync(id, cancellationToken));
    }
}