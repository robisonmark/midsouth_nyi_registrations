using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Models;
using Microsoft.Extensions.Logging;

namespace RoBrosEventsService.Services;

public interface IEventService
{
    Task<bool> CreateEventAsync(Event newEvent);
    Task<Event?> GetEventAsync(Guid id);
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<IEnumerable<EventSlot?>> GetTimeSlotsAsync(Guid id);

    // Reservation (signup) flow
    Task<SlotReservation?> CreateReservationAsync(SlotReservation reservation);
    // Task<SlotReservation?> GetReservationAsync(Guid slotId, Guid participantId);
    // Task<bool> DeleteReservationAsync(Guid slotId, Guid participantId);
    // Task<IEnumerable<SlotReservation>> GetReservationsByEventAsync(Guid eventId);
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

    public async Task<SlotReservation?> CreateReservationAsync(SlotReservation reservation)
    {
        _logger.LogInformation("Creating Reservation for Slot ID: {slotId}", reservation.SlotId);
        return await _repository.CreateReservation(reservation);
    }

    public async Task<bool> CreateEventAsync(Event newEvent)
    {
        _logger.LogInformation("Creating Event: {name}", newEvent.Name);
        return await _repository.CreateEvent(newEvent);
    }
}