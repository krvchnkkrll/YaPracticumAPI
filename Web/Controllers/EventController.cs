using Application.Contracts.Models;
using Application.Contracts.Services;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("api/events")]
public sealed class EventController(
    IEventService eventService,
    IBookingService bookingService) : AppController
{
    /// <summary>
    ///     Получить все события
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<GetEventResponse>), StatusCodes.Status200OK)]
    public ActionResult<PaginatedResult<GetEventResponse>> GetEvents([FromQuery] GetEventsSearchQuery searchQuery, [FromQuery] PaginationQuery paginationQuery)
    {
        return Ok(eventService.GetPaginatedEvents(searchQuery, paginationQuery));
    }

    /// <summary>
    ///     Получить событие
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GetEventResponse> GetEvent([FromRoute] Guid id)
    {
        return Ok(eventService.GetEvent(id));
    }

    /// <summary>
    ///     Создать событие
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    public ActionResult<CreateEventResponse> CreateEvent([FromBody] CreateEventRequest createEventRequest)
    {
        var result = eventService.CreateEvent(createEventRequest);
        return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
    }
    
    /// <summary>
    ///     Создать бронь
    /// </summary>
    [HttpPost("{id:guid}/book")]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<CreateBookingResponse> CreateBooking([FromRoute] Guid id)
    {
        var result = bookingService.CreateBookingAsync(id);
        return AcceptedAtAction(nameof(BookingController.GetBooking), 
            nameof(BookingController).Replace("Controller", ""),
            new { id = result.Id },
            result);
    }
    
    /// <summary>
    ///     Изменить событие
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult UpdateEvent([FromRoute] Guid id, [FromBody] UpdateEventRequest updateEventRequest)
    {
        eventService.UpdateEvent(id, updateEventRequest);
        return NoContent();
    }

    /// <summary>
    ///     Удалить событие
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DeleteEvent([FromRoute] Guid id)
    {
        eventService.DeleteEvent(id);
        return NoContent();
    }
}