using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace RoBrosEventsService.Data;

public class EventRepository : IEventRepository
{
    private readonly ISqlProvider _sqlProvider;
    private readonly string _connectionString;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(string connectionString, ISqlProvider sqlProvider, ILogger<EventRepository> logger)
    {
        _connectionString = connectionString;
        _sqlProvider = sqlProvider;
        _logger = logger;
    }

    public async Task<Event> GetEventById(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(_sqlProvider.GetEventQuery(), connection);
        command.Parameters.AddWithValue("@eventId", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapEventFromReader(reader);
        }

        throw new InvalidOperationException("Failed to get event");
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(string? level = null)
    {
        var events = new List<Event>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        string sql = _sqlProvider.GetAllEventsQuery();
        if (!string.IsNullOrEmpty(level))
        {
            sql += " WHERE level = @level";
        }
        
        using var command = new SqlCommand(sql, connection);
        if (!string.IsNullOrEmpty(level))
        {
            command.Parameters.AddWithValue("@level", level);
        }
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            events.Add(MapEventFromReader(reader));
        }
        return events;
    }

    public async Task<IEnumerable<EventSlot>> GetEventTimeSlots(Guid id)
    {
        var times = new List<EventSlot>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(_sqlProvider.GetEventTimeSlotsQuery(), connection);
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
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(_sqlProvider.CreateReservationQuery(), connection);
        command.Parameters.AddWithValue("@slotId", reservation.SlotId);
        command.Parameters.AddWithValue("@participantId", reservation.ParticipantId);
        command.Parameters.AddWithValue("@reservedName", reservation.ReservedName);
        command.Parameters.AddWithValue("@reservedContact", reservation.ReservedContact);
        command.Parameters.AddWithValue("@Status", "reserved");
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(); 
        
        using var update = new SqlCommand(_sqlProvider.UpdateReservedCountQuery(), connection);
        update.Parameters.AddWithValue("@slotId", reservation.SlotId);
        await update.ExecuteNonQueryAsync();

        return reservation;
    }

    public async Task<bool> CreateEvent(Event newEvent)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(_sqlProvider.CreateEvent(), connection);
        command.Parameters.AddWithValue("@category", newEvent.Category);
        command.Parameters.AddWithValue("@name", newEvent.Name);
        command.Parameters.AddWithValue("@masterStart", newEvent.MasterStart ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@masterEnd", newEvent.MasterEnd ?? (object)DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
        return true;
    }

    // public async Task<bool> CreateEventTimeSlots(EventSlot newTimeSlot)
    // {
    //     // Implementation for creating event time slots would go here
    //     return true;
    // }

    public async Task<SlotReservation?> GetReservationById(Guid reservationId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(_sqlProvider.GetReservationByIdQuery(), connection);
        command.Parameters.AddWithValue("@reservationId", reservationId);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new SlotReservation
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                SlotId = reader.GetGuid(reader.GetOrdinal("slot_id")),
                ParticipantId = reader.GetGuid(reader.GetOrdinal("participant_id")),
                ReservedName = reader.GetString(reader.GetOrdinal("reserved_name")),
                ReservedContact = reader.GetString(reader.GetOrdinal("reserved_contact")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("created_at")).DateTime,
                Church = reader.GetString(reader.GetOrdinal("church"))
            };
        }

        return null;
    }

    private static Event MapEventFromReader(DbDataReader reader)
    {
        return new Event
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Category = reader.GetString(reader.GetOrdinal("category")),
            MasterStart = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("master_start")).DateTime,
            MasterEnd = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("master_end")).DateTime,
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
        };
    }

    private static EventSlot MaptTimeSlotFromReader(DbDataReader reader)
    {
        return new EventSlot
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            EventId = reader.GetGuid(reader.GetOrdinal("event_id")),
            TimeBlockId = reader.GetGuid(reader.GetOrdinal("time_block_id")),
            LocationId = reader.GetString(reader.GetOrdinal("name")),
            StartTime = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("start_time")).DateTime,
            EndTime = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("end_time")).DateTime,
            Capacity = reader.GetInt32(reader.GetOrdinal("capacity")),
            ReservedCount = reader.GetInt32(reader.GetOrdinal("reserved_count")),
            Status = reader.GetString(reader.GetOrdinal("status")),
            Level = reader.GetString(reader.GetOrdinal("level"))
        };
    }

    public async Task<IEnumerable<string>> GetAllAgeGroupsAsync()
    {
        var ageGroups = new List<string>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // This assumes you have an EventSlot or EventTimeBlock table with an AgeGroup or Level column
        // Adjust the query as needed for your schema
        var sql = "SELECT DISTINCT level FROM event_time_blocks WHERE level IS NOT NULL AND level <> '' ORDER BY level";
        
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            ageGroups.Add(reader.GetString(0));
        }
        return ageGroups;
    }
    
    public async Task<IEnumerable<string>> GetAgeGroupsByEventAsync(Guid eventId)
    {
        var ageGroups = new List<string>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // This assumes you have an EventTimeBlock or similar table with event_id and level columns
        // Adjust the query as needed for your schema
        var sql = "SELECT DISTINCT level FROM event_time_blocks WHERE event_id = @eventId AND level IS NOT NULL AND level <> '' ORDER BY level";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eventId", eventId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ageGroups.Add(reader.GetString(0));
        }
        return ageGroups;
    }
}