-- Enable UUID generation (only once per database)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

------------------------------------------------------------
-- EVENTS
------------------------------------------------------------
CREATE TABLE events (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name text NOT NULL,
    category text NOT NULL,
    type text, -- e.g., Vocal, Art, Speech, etc.
    master_start timestamptz,
    master_end timestamptz,
    is_active boolean DEFAULT true,
    created_at timestamptz DEFAULT now(),
    updated_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- LOCATIONS
------------------------------------------------------------
CREATE TABLE locations (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name text NOT NULL,
    building text,
    created_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- EVENT TIME BLOCKS
------------------------------------------------------------
-- Each event can have multiple time blocks (morning, afternoon, etc.)
-- Each block may optionally be scoped to a level (EY/SY/All)
------------------------------------------------------------
CREATE TABLE event_time_blocks (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id uuid REFERENCES events(id) ON DELETE CASCADE,
    location_id uuid REFERENCES locations(id) ON DELETE CASCADE,
    start_time timestamptz NOT NULL,
    end_time timestamptz NOT NULL,
    slot_duration_minutes int DEFAULT 10,
    manual_slots boolean DEFAULT false,
    level text, -- e.g. 'EY', 'SY', or NULL for all
    created_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- EVENT SLOTS
------------------------------------------------------------
-- Represents individual signup times within a block
------------------------------------------------------------
CREATE TABLE event_slots (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id uuid REFERENCES events(id) ON DELETE CASCADE,
    time_block_id uuid REFERENCES event_time_blocks(id) ON DELETE CASCADE,
    location_id uuid REFERENCES locations(id) ON DELETE CASCADE,
    start_time timestamptz NOT NULL,
    end_time timestamptz NOT NULL,
    capacity int DEFAULT 1,
    reserved_count int DEFAULT 0,
    status text DEFAULT 'open',
    created_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- PARTICIPANTS
------------------------------------------------------------
-- Represents a performer or team proxy
------------------------------------------------------------
CREATE TABLE participants (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    external_participant_id text,
    first_name text,
    last_name text,
    grade int,                -- used for grade-based rules
    birth_date date,          -- used for age-based rules
    metadata jsonb,
    created_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- EVENT RULES
------------------------------------------------------------
-- Defines eligibility (by grade or age) and team/individual structure
------------------------------------------------------------
CREATE TABLE event_rules (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id uuid REFERENCES events(id) ON DELETE CASCADE,
    level text,  -- e.g. 'Early Youth', 'Senior Youth', or 'All'
    rule_basis text CHECK (rule_basis IN ('grade','age')) DEFAULT 'grade',
    min_grade int,
    max_grade int,
    min_age int,
    max_age int,
    team_event boolean DEFAULT false,
    individual_event boolean DEFAULT true,
    notes text,
    created_at timestamptz DEFAULT now()
);

------------------------------------------------------------
-- SLOT RESERVATIONS
------------------------------------------------------------
-- A participant (or group) reserving a slot
------------------------------------------------------------
CREATE TABLE slot_reservations (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    slot_id uuid REFERENCES event_slots(id) ON DELETE CASCADE,
    participant_id uuid REFERENCES participants(id) ON DELETE CASCADE,
    reserved_name text,       -- sponsor/coach name if group signup
    reserved_contact text,
    status text DEFAULT 'reserved',
    created_at timestamptz DEFAULT now(),
    version int DEFAULT 1
);

------------------------------------------------------------
-- INDEXES
------------------------------------------------------------
CREATE INDEX idx_events_active ON events(is_active);
CREATE INDEX idx_slots_event_id ON event_slots(event_id);
CREATE INDEX idx_slots_status ON event_slots(status);
CREATE INDEX idx_rules_event_id ON event_rules(event_id);
CREATE INDEX idx_participants_grade ON participants(grade);
CREATE INDEX idx_participants_birth_date ON participants(birth_date);
CREATE INDEX idx_time_blocks_event_id ON event_time_blocks(event_id);


------------------------------------------------------------
-- LOCATIONS
------------------------------------------------------------
INSERT INTO locations (id, name, building)
VALUES
  (uuid_generate_v4(), 'Sanctuary', 'Main'),
  (uuid_generate_v4(), 'The Well', 'Main'),
  (uuid_generate_v4(), 'The Mount', 'Main'),
  (uuid_generate_v4(), 'Choir Room', 'Lower Level'),
  (uuid_generate_v4(), 'LL01', 'Lower Level'),
  (uuid_generate_v4(), 'LL02', 'Lower Level'),
  (uuid_generate_v4(), 'Lobby', 'Main Entry');

------------------------------------------------------------
-- EVENTS
------------------------------------------------------------
INSERT INTO events (id, name, category, type, master_start, master_end)
VALUES
  (uuid_generate_v4(), 'Oil/Acrylic', 'Art', 'Visual', '2025-11-21 20:00:00-06', '2025-11-22 00:00:00-06'),
  (uuid_generate_v4(), 'Watercolor', 'Art', 'Visual', '2025-11-22 08:00:00-06', '2025-11-22 12:00:00-06'),
  (uuid_generate_v4(), 'Vocal Solo, Female', 'Vocal Music', 'Performance', '2025-11-22 13:00:00-06', '2025-11-22 17:00:00-06');

------------------------------------------------------------
-- EVENT RULES (EY = grades 6–8, SY = grades 9–12)
------------------------------------------------------------
-- Oil/Acrylic
INSERT INTO event_rules (event_id, level, rule_basis, min_grade, max_grade)
SELECT e.id, 'Early Youth', 'grade', 6, 8 FROM events e WHERE e.name = 'Oil/Acrylic';
INSERT INTO event_rules (event_id, level, rule_basis, min_grade, max_grade)
SELECT e.id, 'Senior Youth', 'grade', 9, 12 FROM events e WHERE e.name = 'Oil/Acrylic';

-- Watercolor
INSERT INTO event_rules (event_id, level, rule_basis, min_grade, max_grade)
SELECT e.id, 'Early Youth', 'grade', 6, 8 FROM events e WHERE e.name = 'Watercolor';
INSERT INTO event_rules (event_id, level, rule_basis, min_grade, max_grade)
SELECT e.id, 'Senior Youth', 'grade', 9, 12 FROM events e WHERE e.name = 'Watercolor';

-- Vocal Solo, Female (both levels)
INSERT INTO event_rules (event_id, level, rule_basis, min_grade, max_grade)
SELECT e.id, 'All', 'grade', 6, 12 FROM events e WHERE e.name = 'Vocal Solo, Female';

------------------------------------------------------------
-- EVENT TIME BLOCKS
------------------------------------------------------------
-- Example: Oil/Acrylic (Sanctuary, EY 8–10 AM, SY 10–12 PM)
INSERT INTO event_time_blocks (event_id, location_id, start_time, end_time, slot_duration_minutes, level)
SELECT e.id, l.id, '2025-11-22 08:00:00-06', '2025-11-22 10:00:00-06', 10, 'EY'
FROM events e JOIN locations l ON l.name = 'Sanctuary' WHERE e.name = 'Oil/Acrylic';

INSERT INTO event_time_blocks (event_id, location_id, start_time, end_time, slot_duration_minutes, level)
SELECT e.id, l.id, '2025-11-22 10:00:00-06', '2025-11-22 12:00:00-06', 10, 'SY'
FROM events e JOIN locations l ON l.name = 'Sanctuary' WHERE e.name = 'Oil/Acrylic';

-- Example: Watercolor (LL01, All 8–12)
INSERT INTO event_time_blocks (event_id, location_id, start_time, end_time, slot_duration_minutes, level)
SELECT e.id, l.id, '2025-11-22 08:00:00-06', '2025-11-22 12:00:00-06', 10, 'All'
FROM events e JOIN locations l ON l.name = 'LL01' WHERE e.name = 'Watercolor';

-- Example: Vocal Solo, Female (The Well, All 1–5 PM)
INSERT INTO event_time_blocks (event_id, location_id, start_time, end_time, slot_duration_minutes, level)
SELECT e.id, l.id, '2025-11-22 13:00:00-06', '2025-11-22 17:00:00-06', 10, 'All'
FROM events e JOIN locations l ON l.name = 'The Well' WHERE e.name = 'Vocal Solo, Female';

------------------------------------------------------------
-- EVENT SLOTS (generated manually for example)
------------------------------------------------------------
-- Oil/Acrylic EY 8:00–10:00 every 10 minutes
DO $$
DECLARE
    event_id uuid;
    block_id uuid;
    location_id uuid;
    t timestamptz;
BEGIN
    SELECT e.id, b.id, b.location_id INTO event_id, block_id, location_id
    FROM events e
    JOIN event_time_blocks b ON b.event_id = e.id
    WHERE e.name = 'Oil/Acrylic' AND b.level = 'EY';

    t := '2025-11-22 08:00:00-06';
    WHILE t < '2025-11-22 10:00:00-06' LOOP
        INSERT INTO event_slots (event_id, time_block_id, location_id, start_time, end_time)
        VALUES (event_id, block_id, location_id, t, t + interval '10 minutes');
        t := t + interval '10 minutes';
    END LOOP;
END $$;
