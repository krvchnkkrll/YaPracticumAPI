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
    public ActionResult<GetBookingResponse> GetBooking([FromRoute] Guid id)
    {
        return Ok(bookingService.GetBookingByIdAsync(id));
    }
}