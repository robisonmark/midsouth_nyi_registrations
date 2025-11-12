CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Create ENUM type for CompetitionStatus (optional)
-- need to update this... ask Matt.  I think he said config or join table
CREATE TYPE competition_status AS ENUM ('Competing', 'Spectating');
CREATE TYPE participant_role AS ENUM('Student', 'Chaperone', 'Volunteer');

-- 3. Create Address table
CREATE TABLE IF NOT EXISTS addresses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(), -- Assuming UUID for consistency
    street_address_1 VARCHAR(255) NOT NULL,
    street_address_2 VARCHAR(255),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    postal_code VARCHAR (25) NOT NULL,
    country VARCHAR(100) NOT NULL,

    created_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    created_by VARCHAR(255) NOT NULL,       -- BaseEntity field
    updated_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    updated_by VARCHAR(255),                -- BaseEntity field
    version INT NOT NULL                    -- BaseEntity field
);

CREATE UNIQUE INDEX idx_unique_address ON addresses (
    street_address_1,
    city,
    postal_code
);

-- 4. Create Church table
CREATE TABLE IF NOT EXISTS churches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    address_id UUID,

    created_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    created_by VARCHAR(255) NOT NULL,       -- BaseEntity field
    updated_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    updated_by VARCHAR(255),                -- BaseEntity field
    version INT NOT NULL                    -- BaseEntity field
);

-- 5. Create Registrant table
CREATE TABLE IF NOT EXISTS registrants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    given_name VARCHAR(255) NOT NULL,
    family_name VARCHAR(255) NOT NULL,
    district_id UUID,
    participant_role TEXT NOT NULL, -- student/chaperone/volunteer
    church_id UUID NOT NULL,
    youth_leader_email VARCHAR(255),
    youth_leader_first_name VARCHAR(255),
    youth_leader_last_name VARCHAR(255),
    address_id UUID NOT NULL,
    cell_number VARCHAR(15),
    email VARCHAR(255),
    birthday DATE NOT NULL,
    gender VARCHAR(10),
    shirt_size VARCHAR(10),
    price FLOAT NOT NULL,
    paid BOOLEAN NOT NULL,
    notes TEXT,
    submission_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45) NOT NULL,

    created_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    created_by VARCHAR(255) NOT NULL,       -- BaseEntity field
    updated_at TIMESTAMP DEFAULT now(),     -- BaseEntity field
    updated_by VARCHAR(255),                -- BaseEntity field
    version INT NOT NULL,                    -- BaseEntity field

    CONSTRAINT fk_church FOREIGN KEY (church_id) REFERENCES churches(id),
    CONSTRAINT fk_address FOREIGN KEY (address_id) REFERENCES addresses(id)
);

CREATE UNIQUE INDEX idx_unique_reqistrant ON registrants (
    given_name,
    family_name,
    birthday
);

-- 6. Create Indexes
-- CREATE INDEX idx_spectator_registrant_id ON spectator(registrant_id);
CREATE TABLE IF NOT EXISTS students (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    registrant_id UUID NOT NULL,
    medical_conditions TEXT,
    dietary_restrictions TEXT,
    allergies TEXT[], -- Array type
    food_allergies TEXT[],
    medications TEXT[],
    guardian_first_name VARCHAR(255) NOT NULL,
    guardian_last_name VARCHAR(255) NOT NULL,
    guardian_home_phone VARCHAR(20) NOT NULL,
    guardian_work_phone VARCHAR(20) NOT NULL,
    guardian_contact_phone VARCHAR(20) NOT NULL,
    insurance_company VARCHAR(255) NOT NULL,
    policy_id VARCHAR(255) NOT NULL,

    competition_status VARCHAR(50) NOT NULL DEFAULT 'Competing', -- Status with default value
    quizzing BOOLEAN NOT NULL DEFAULT FALSE, -- Whether they are quizzing
    art_events TEXT[], -- List of art categories, stored as array
    creative_ministries_events TEXT[], -- List of creative ministries categories, stored as array
    creative_writing_events TEXT[], -- List of creative writing categories, stored as array
    speech_events TEXT[], -- List of speech categories, stored as array
    academic_events TEXT[], -- List of academic categories, stored as array
    vocal_music_events TEXT[], -- List of vocal music categories, stored as array
    instrumental_music_events TEXT[], -- List of instrumental music categories, stored as array
    individual_sports_events TEXT[], -- List of individual sport categories, stored as array
    team_sports_events TEXT[], -- List of team sport categories, stored as array
    attending_tnt_at_tnu BOOLEAN NOT NULL, -- Whether they are attending TNT at TNU

    CONSTRAINT fk_registrant FOREIGN KEY (registrant_id) REFERENCES registrants(id)
);

-- -- Competitor Table
-- CREATE TABLE Competitor (
--     Id UUID PRIMARY KEY,
--     RegistrantId UUID NOT NULL,
--     District INT NOT NULL,
--     CompetitionStatus VARCHAR(50) NOT NULL DEFAULT 'Competing',
--     Quizzing BOOLEAN NOT NULL DEFAULT FALSE,
--     AttendingTNTatTNU BOOLEAN NOT NULL,
--     CONSTRAINT fk_registrant FOREIGN KEY (RegistrantId) REFERENCES Registrants(Id) ON DELETE CASCADE
-- );

-- -- CompetitorCategories Table for storing categories
-- CREATE TABLE CompetitorCategories (
--     CompetitorId UUID NOT NULL,
--     CategoryType VARCHAR(50) NOT NULL,
--     CategoryName VARCHAR(255) NOT NULL,
--     CONSTRAINT fk_competitor FOREIGN KEY (CompetitorId) REFERENCES Competitor(Id) ON DELETE CASCADE
-- );

-- 2. Create Join Tables
CREATE TABLE IF NOT EXISTS registrant_participant_roles (
    registrant_id UUID DEFAULT uuid_generate_v4(),
    participant_role participant_role NOT NULL,
    PRIMARY KEY (registrant_id, participant_role),
    CONSTRAINT fk_registrant FOREIGN KEY (registrant_id) REFERENCES registrants(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS registrant_competition_statuses (
    registrant_id UUID DEFAULT uuid_generate_v4(),
    competition_status competition_status NOT NULL,
    PRIMARY KEY (registrant_id, competition_status),
    CONSTRAINT fk_registrant FOREIGN KEY (registrant_id) REFERENCES registrants(id) ON DELETE CASCADE
);



