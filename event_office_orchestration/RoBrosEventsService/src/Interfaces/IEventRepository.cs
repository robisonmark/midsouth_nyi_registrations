using RoBrosEventsService.Models;

namespace RoBrosEventsService.Interfaces;

public interface IEventRepository
{
    Task<Event> GetEventById(Guid id);
    Task<IEnumerable<Event>> GetAllEventsAsync(string? level = null);
    Task<IEnumerable<EventSlot>> GetEventTimeSlots(Guid id);
    Task<bool> CreateEvent(Event newEvent);
    
    Task<SlotReservation> CreateReservation(SlotReservation reservation);
    Task<SlotReservation?> GetReservationById(Guid reservationId);
}

public interface ISqlProvider
{
    string GetEventQuery();
    string GetAllEventsQuery();
    string GetEventTimeSlotsQuery();
    string CreateReservationQuery();
    string CreateEvent();
    string GetReservationByIdQuery();
    string UpdateReservedCountQuery();
    
}