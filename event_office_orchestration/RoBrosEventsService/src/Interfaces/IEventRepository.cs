using RoBrosEventsService.Models;

namespace RoBrosEventsService.Interfaces;

public interface IEventRepository
{
    Task<Event> GetEventById(Guid id);
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<IEnumerable<EventSlot>> GetEventTimeSlots(Guid id);
}

public interface ISqlProvider
{
    string GetEventQuery();
    string GetAllEventsQuery();
    string GetEventTimeSlotsQuery();
    
}