using System.ComponentModel.DataAnnotations;

namespace RoBrosEventsService.Models;

// For Events and event signups.  An Event Occurs for a category and level
// Each of these events will have an event start time and a run time. 
// At event creation the time slots would need to be created.
// A user who is signed up for the event and category level would 
// see the timeslots that are still available. 

// Will need to validate a user against registrations ( A student not registered for an event shouldn't to sign up for a time slot.  
// Do I need to scope this as the current user (will there be a current user)
// Should there be a Get my Events

public class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public DateTime? MasterStart { get; set; }
    public DateTime? MasterEnd { get; set; }
    public bool IsActive { get; set; } = true;

    // public ICollection<EventSlot> Slots { get; set; } = new List<EventSlot>();
}

public class EventSlot
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int Capacity { get; set; } = 1;
    public int ReservedCount { get; set; } = 0;
    public string Status { get; set; } = "open"; // open, full, closed

    public ICollection<SlotReservation> Reservations { get; set; } = new List<SlotReservation>();
}

public class SlotReservation
{
    public Guid Id { get; set; }

    public Guid SlotId { get; set; }
    public EventSlot? Slot { get; set; }

    public Guid? ParticipantId { get; set; }
    // public Participant? Participant { get; set; }

    public string ReservedName { get; set; } = default!;
    public string ReservedContact { get; set; } = default!;

    public string Status { get; set; } = "reserved"; // reserved, cancelled, checked_in

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EventSignUp
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int RegistrantId { get; set; } 

    [Required]
    public Guid TimeSlotId { get; set; }

}

public class EventLevel
{
    public Guid Id { get; set; }
    
    [Required]
    public string LevelName { get; set; }

    // Should this have the confgurations based off of age/grade level
}

public class EventType
{
    // Should this even exidost Here? 
    public Guid Id { get; set; }
    
    [Required]
    public string TypeName { get; set; }
}