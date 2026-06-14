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
    public async Task<ActionResult<PaginatedResult<GetEventResponse>>> GetEventsAsync(
        [FromQuery] GetEventsSearchQuery searchQuery, 
        [FromQuery] PaginationQuery paginationQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await eventService.GetPaginatedAsync(searchQuery, paginationQuery, cancellationToken));
    }

    /// <summary>
    ///     Получить событие
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetEventResponse>> GetEventAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await eventService.GetEventByIdAsync(id, cancellationToken));
    }

    /// <summary>
    ///     Создать событие
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateEventResponse>> CreateEventAsync(
        [FromBody] CreateEventRequest createEventRequest,
        CancellationToken cancellationToken)
    {
        var eventEntity = await eventService.CreateEventAsync(
            createEventRequest,
            cancellationToken);

        return CreatedAtRoute(
            nameof(GetEventAsync),
            new { id = eventEntity.Id },
            eventEntity);
    }
    
    /// <summary>
    ///     Создать бронь
    /// </summary>
    [HttpPost("{id:guid}/book")]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateBookingResponse>> CreateBookingAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var eventEntity = await bookingService.CreateBookingAsync(id, cancellationToken);
        
        return AcceptedAtAction(nameof(BookingController.GetBookingAsync), 
            nameof(BookingController).Replace("Controller", ""),
            new { id = eventEntity.Id },
            eventEntity);
    }
    
    /// <summary>
    ///     Изменить событие
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateEventAsync([FromRoute] Guid id, 
        [FromBody] UpdateEventRequest updateEventRequest, CancellationToken cancellationToken)
    {
        await eventService.UpdateEventAsync(id, updateEventRequest, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Удалить событие
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEvent([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await eventService.DeleteEventAsync(id, cancellationToken);
        return NoContent();
    }
}