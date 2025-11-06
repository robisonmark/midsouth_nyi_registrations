using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace RoBrosEventsService.Data;

public class EventRepository : IEventRepository
{
    private readonly ISqlProvider _sqlProvider;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(NpgsqlDataSource dataSource, ISqlProvider sqlProvider, ILogger<EventRepository> logger)
    {
        _dataSource = dataSource;
        _sqlProvider = sqlProvider;
        _logger = logger;
    }

    public async Task<Event> GetEventById(Guid id)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.GetEventQuery(), connection);
        command.Parameters.AddWithValue("@eventId", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapEventFromReader(reader);
        }

        throw new InvalidOperationException("Failed to create address");
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        var events = new List<Event>();
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.GetAllEventsQuery(), connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            events.Add(MapEventFromReader(reader));
        }
        return events;
        // throw new InvalidOperationException("Failed to create address");
    }

    public async Task<IEnumerable<EventSlot>> GetEventTimeSlots(Guid id)
    {
        var times = new List<EventSlot>();
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.GetEventTimeSlotsQuery(), connection);
        
        command.Parameters.AddWithValue("@eventId", id);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            times.Add(MaptTimeSlotFromReader(reader));
        }
        return times;
    }

    public async Task<SlotReservation> CreateReservation(SlotReservation reservation)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.CreateReservationQuery(), connection);

        command.Parameters.AddWithValue("@slotId", reservation.SlotId);
        command.Parameters.AddWithValue("@participantId", reservation.ParticipantId);
        command.Parameters.AddWithValue("@reservedName", reservation.ReservedName);
        command.Parameters.AddWithValue("@reservedContact", reservation.ReservedContact);
        command.Parameters.AddWithValue("@Status", "reserved");
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(); 

        return reservation;
    }

    public async Task<bool> CreateEvent(Event newEvent)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.CreateEvent(), connection);

        command.Parameters.AddWithValue("@category", newEvent.Category);
        command.Parameters.AddWithValue("@name", newEvent.Name);
        command.Parameters.AddWithValue("@masterStart", newEvent.MasterStart ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@masterEnd", newEvent.MasterEnd ?? (object)DBNull.Value);
        await command.ExecuteNonQueryAsync();
        return true;

    }

    public async Task<bool> CreateEventTimeSlots(EventSlot newTimeSlot)
    {
        // Implementation for creating event time slots would go here
        return true;
    }

    private static Event MapEventFromReader(DbDataReader reader)
    {
        return new Event
        {
            Id = reader.GetGuid("id"),
            Name = reader.GetString("name"),
            Category = reader.GetString("category"),
            MasterStart = reader.GetDateTime("master_start"),
            MasterEnd = reader.GetDateTime("master_end"),
            IsActive = reader.GetBoolean("is_active")
        };
    }

    private static EventSlot MaptTimeSlotFromReader(DbDataReader reader)
    {
        return new EventSlot
        {
            Id = reader.GetGuid("id"),
            EventId = reader.GetGuid("event_id"),
            TimeBlockId = reader.GetGuid("time_block_id"),
            LocationId = reader.GetGuid("location_id"),
            StartTime = reader.GetDateTime("start_time"),
            EndTime = reader.GetDateTime("end_time"),
            Capacity = reader.GetInt32("capacity"),
            ReservedCount = reader.GetInt32("reserved_count"),
            Status = reader.GetString("status"),
        };
    }

}