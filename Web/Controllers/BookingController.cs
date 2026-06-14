using Application.Contracts.Models;
using Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("api/bookings")]
public sealed class BookingController(IBookingService bookingService) : AppController
{
    /// <summary>
    ///     Получить бронь
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBookingResponse>> GetBookingAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await bookingService.GetBookingByIdAsync(id, cancellationToken));
    }
}