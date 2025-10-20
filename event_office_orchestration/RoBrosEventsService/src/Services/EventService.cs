using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Models;
using Microsoft.Extensions.Logging;

namespace RoBrosEventsService.Services;

public interface IEventService
{
    Task<Event?> GetEventAsync(Guid id);
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<IEnumerable<EventSlot?>> GetTimeSlotsAsync(Guid id);
}

public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository repository, ILogger<EventService> logger)
    {
        _repository = repository;
        _logger = logger; 
    } 

    // Get An Event
    public async Task<Event?> GetEventAsync(Guid id)
    {
        _logger.LogInformation("Getting Event with ID: {id}", id);
        return await _repository.GetEventById(id);
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _repository.GetAllEventsAsync();
    }

    public async Task<IEnumerable<EventSlot?>> GetTimeSlotsAsync(Guid id)
    {
        return await _repository.GetEventTimeSlots(id);
    }

    
}