using Events.Application.Interfaces.Services;
using Events.Application.Models;
using Events.Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Presentation.Controllers;

[Route("api/events")]
public sealed class EventController(
    IEventService eventService) : AppController
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
    [HttpGet("{id}", Name = nameof(GetEventAsync))]
    [ProducesResponseType(typeof(GetEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetEventResponse>> GetEventAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await eventService.GetEventByIdAsync(id, cancellationToken));
    }

    /// <summary>
    ///     Получить топ-10 событий
    /// </summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(IEnumerable<GetEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GetEventResponse>>> GetTopEventsAsync(CancellationToken cancellationToken)
    {
        return Ok(await eventService.GetTopEventsAsync(cancellationToken));
    }


    /// <summary>
    ///     Создать событие
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    ///     Изменить событие
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UpdateEventResponse), StatusCodes.Status205ResetContent)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateEventAsync([FromRoute] Guid id,
        [FromBody] UpdateEventRequest updateEventRequest, CancellationToken cancellationToken)
    {
        var result = await eventService.UpdateEventAsync(id, updateEventRequest, cancellationToken);
        
        return StatusCode(StatusCodes.Status205ResetContent, result);
    }

    /// <summary>
    ///     Удалить событие
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEvent([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await eventService.DeleteEventAsync(id, cancellationToken);
        return NoContent();
    }
}