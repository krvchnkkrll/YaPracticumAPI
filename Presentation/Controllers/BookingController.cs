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
    public async Task<ActionResult<GetBookingResponse>> GetBookingByIdAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await bookingService.GetBookingByIdAsync(id, cancellationToken));
    }

    /// <summary>
    ///     Получить бронь
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBookingResponse>> DeleteBookingAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await bookingService.DeleteBookingAsync(id, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    ///     Создать бронь
    /// </summary>
    [HttpPost("{id:guid}/book")]
    [Authorize]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateBookingResponse>> CreateBookingAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var eventEntity = await bookingService.CreateBookingAsync(id, cancellationToken);

        return AcceptedAtRoute(
            nameof(BookingController.GetBookingByIdAsync),
            new { id = eventEntity.Id },
            eventEntity);
    }
}