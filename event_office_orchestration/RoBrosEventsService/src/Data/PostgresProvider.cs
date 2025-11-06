using RoBrosEventsService.Interfaces;

namespace RoBrosEventsService.Data;

public class PostgresProvider : ISqlProvider
{
    public virtual string GetAllEventsQuery()
    {
        return @"
            SELECT events.*
            FROM events;
        ";
    }

    public virtual string GetEventQuery()
    {
        return @"
            SELECT * 
            FROM events 
            WHERE id = @eventId
        ";
    }

    public virtual string CreateEvent()
    {
        return @"
            INSERT INTO events (id, category, name, master_start, master_end, is_active) 
            VALUES
            (uuid_generate_v4(), @category, @name, @masterStart, @masterEnd, 'true')
        ";
    }

    public virtual string CreateEventTimes()
    {
        return @"
            INSERT INTO event_times (event_id, start_time, end_time, created_by, version)
            VALUES
            (@eventId, @startTime, @endTime, @createdBy, 1);

        ";
    }

    public virtual string GetEventTimeSlotsQuery()
    {
        return @"
        SELECT * 
        FROM event_slots
        INNER JOIN event_time_blocks
            ON event_slots.time_block_id = event_time_blocks.id
        WHERE event_slots.event_id = @eventId
        ORDER BY event_slots.start_time ASC;
        ";
    }

    public virtual string CreateSignUpQuery()
    {
        return @"
            INSERT INTO event_signup (student_id, event_slot)
            VALUES
                (@registrantId, @eventSlot);
        ";
    }

    public virtual string DeleteSignUpQuery()
    {
        return @"
            DELETE FROM event_signup 
            WHERE student_id = @registrantId 
            AND event_slot = @eventSlot;
        ";
    }

    public virtual string CreateTimeSlotsWuery()
    {
        // Future, to be called as events are added and configured
        return @"";
    }

    public virtual string GetTimeSlotsQuery()
    {
        return @"
            SELECT * 
            FROM event_times
            WHERE event_id = @eventId
            ORDER BY start_time ASC;
        ";
    }

    public virtual string GetEventTypesQuery()
    {
        return @"
            SELECT *
            FROM event_types
        ";
    }

    public virtual string GetEventLevelsQuery()
    {
        return @"
            SELECT *
            FROM event_levels;
        ";
    }

    public virtual string GetEventsByLevelQuery()
    {
        return @"
            SELECT * 
            FROM events 
            WHERE event_level = @eventLevelId;
        ";
    }

    public virtual string CreateReservationQuery()
    {
        return @"
            INSERT INTO slot_reservations
                (slot_id, participant_id, reserved_name, reserved_contact, status, created_at)
            VALUES
                (@SlotId, @ParticipantId, @ReservedName, @ReservedContact, @Status, @CreatedAt)
            RETURNING *;
        ";
    }

}

