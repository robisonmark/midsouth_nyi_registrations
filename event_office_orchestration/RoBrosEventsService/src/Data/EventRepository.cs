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
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            // events.Add(MapEventFromReader(reader));
        }
        return times;
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

    private static TimeSlots

}