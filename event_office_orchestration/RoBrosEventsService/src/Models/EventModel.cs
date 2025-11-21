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
    public string? Type { get; set; }
    public DateTime? MasterStart { get; set; }
    public DateTime? MasterEnd { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EventSlot
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TimeBlockId { get; set; }
    public string LocationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; } = 1;
    public int ReservedCount { get; set; } = 0;
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; }
    public string Level { get; set; } = default!;
}

public class SlotReservation
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public Guid ParticipantId { get; set; }
    public string ReservedName { get; set; } = default!;
    public string ReservedContact { get; set; } = default!;
    public string Status { get; set; } = "reserved";
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; } = 1;
    public string? Church { get; set; }
}

public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Building { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventTimeBlock
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string? Level { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Participant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int? Grade { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventRule
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string? Eligibility { get; set; }
    public string? Structure { get; set; }
    public DateTime CreatedAt { get; set; }
}