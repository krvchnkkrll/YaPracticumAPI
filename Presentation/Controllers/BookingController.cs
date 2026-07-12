using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Route("api/bookings")]
public sealed class BookingController(IBookingService bookingService) : AppController
{
    /// <summary>
    ///     Получить бронь
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}", Name = nameof(GetBookingByIdAsync))]
    [ProducesResponseType(typeof(GetBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBookingResponse>> GetBookingByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await bookingService.GetBookingByIdAsync(id, cancellationToken));
    }
}