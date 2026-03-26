using Application.Contracts.Models;
using Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("api/events")]
public sealed class EventController(IEventService eventService) : AppController
{
    /// <summary>
    ///     Получить все события
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEventResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<GetEventResponse>> Get()
    {
        return Ok(eventService.GetEvents());
    }

    /// <summary>
    ///     Получить событие
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GetEventResponse> Get([FromRoute] Guid id)
    {
        return Ok(eventService.GetEvent(id));
    }

    /// <summary>
    ///     Создать событие
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    public ActionResult<CreateEventResponse> Post([FromBody] CreateEventRequest createEventRequest)
    {
        var result = eventService.CreateEvent(createEventRequest);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    
    /// <summary>
    ///     Изменить событие
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Put([FromRoute] Guid id, [FromBody] UpdateEventRequest updateEventRequest)
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
    public ActionResult Delete([FromRoute] Guid id)
    {
        eventService.DeleteEvent(id);
        return NoContent();
    }
}