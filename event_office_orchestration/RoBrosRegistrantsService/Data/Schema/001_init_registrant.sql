-- 1. Create ENUM type for CompetitionStatus (optional)
CREATE TYPE competition_status AS ENUM ('Competing', 'Spectating');
CREATE TYPE particpant_role AS ENUM('Student', 'Chaperone', 'Volunteer');

-- 2. Create Address table
CREATE TABLE IF NOT EXISTS Address (
    Id UUID PRIMARY KEY, -- Assuming UUID for consistency
    StreetAddress1 VARCHAR(255) NOT NULL,
    StreetAddress2 VARCHAR(255),
    Locality VARCHAR(100) NOT NULL,
    PostalCode INT NOT NULL,
    Country VARCHAR(100) NOT NULL,
    AdministrativeAreaLevel VARCHAR(100) NOT NULL,

    CreatedAt TIMESTAMP NOT NULL,          -- BaseEntity field
    CreatedBy VARCHAR(255) NOT NULL,       -- BaseEntity field
    UpdatedAt TIMESTAMP,                   -- BaseEntity field
    UpdatedBy VARCHAR(255),                -- BaseEntity field
    Version INT NOT NULL,                  -- BaseEntity field
    EntityData TEXT                        -- BaseEntity field
);

-- 3. Create Church table
CREATE TABLE IF NOT EXISTS Church (
    Id UUID PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    AddressId UUID,

    CreatedAt TIMESTAMP NOT NULL,          -- BaseEntity field
    CreatedBy VARCHAR(255) NOT NULL,       -- BaseEntity field
    UpdatedAt TIMESTAMP,                   -- BaseEntity field
    UpdatedBy VARCHAR(255),                -- BaseEntity field
    Version INT NOT NULL,                  -- BaseEntity field
    EntityData TEXT,                       -- BaseEntity field

    CONSTRAINT fk_address FOREIGN KEY (AddressId) REFERENCES Address(Id)
);

-- 4. Create Registrant table
CREATE TABLE IF NOT EXISTS Registrant (
    Id UUID PRIMARY KEY,
    GivenName VARCHAR(255) NOT NULL,
    FamilyName VARCHAR(255) NOT NULL,
    ParticpantRole particpant_role NOT NULL, -- student/chaperone/volunteer
    ChurchId UUID NOT NULL,
    YouthLeaderEmail VARCHAR(255),
    YouthLeaderFirstName VARCHAR(255),
    YouthLeaderLastName VARCHAR(255),
    AddressId UUID NOT NULL,
    Mobile VARCHAR(15),
    Email VARCHAR(255),
    Birthday DATE NOT NULL,
    Gender VARCHAR(10),
    ShirtSize VARCHAR(10),
    Price FLOAT NOT NULL,
    Paid BOOLEAN NOT NULL,
    Notes TEXT,
    SubmissionDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IPAddress VARCHAR(45) NOT NULL,

    CreatedAt TIMESTAMP NOT NULL,          -- BaseEntity field
    CreatedBy VARCHAR(255) NOT NULL,       -- BaseEntity field
    UpdatedAt TIMESTAMP,                   -- BaseEntity field
    UpdatedBy VARCHAR(255),                -- BaseEntity field
    Version INT NOT NULL,                  -- BaseEntity field
    EntityData TEXT,                       -- BaseEntity field

    CONSTRAINT fk_church FOREIGN KEY (ChurchId) REFERENCES Church(Id),
    CONSTRAINT fk_address FOREIGN KEY (AddressId) REFERENCES Address(Id)
);

-- 5. Create Student table
CREATE TABLE IF NOT EXISTS Student (
    Id UUID PRIMARY KEY, -- Same as Registrant(Id) for inheritance
    ParticpantRole particpant_role NOT NULL DEFAULT 'Student',
    CompetitionStatus VARCHAR(50) NOT NULL,
    MedicalConditions TEXT,
    DietaryRestrictions TEXT,
    Allergies TEXT[], -- Array type
    FoodAllergies TEXT[],
    Medications TEXT[],
    GuardianFirstName VARCHAR(255) NOT NULL,
    GuardianLastName VARCHAR(255) NOT NULL,
    GuardianHomePhone VARCHAR(20) NOT NULL,
    GuardianWorkPhone VARCHAR(20) NOT NULL,
    GuardianContactPhone VARCHAR(20) NOT NULL,
    InsuranceCompany VARCHAR(255) NOT NULL,
    PolicyId VARCHAR(255) NOT NULL,

    CreatedAt TIMESTAMP NOT NULL,          -- BaseEntity field
    CreatedBy VARCHAR(255) NOT NULL,       -- BaseEntity field
    UpdatedAt TIMESTAMP,                   -- BaseEntity field
    UpdatedBy VARCHAR(255),                -- BaseEntity field
    Version INT NOT NULL,                  -- BaseEntity field
    EntityData TEXT,                       -- BaseEntity field

    CONSTRAINT fk_registrant_student FOREIGN KEY (Id) REFERENCES Registrant(Id)
);

-- 6. Create Spectator table
CREATE TABLE IF NOT EXISTS Spectator (
    Id UUID PRIMARY KEY,
    RegistrantId UUID NOT NULL,
    District INT NOT NULL,
    CompetitionStatus competition_status NOT NULL DEFAULT 'Spectating',

    CreatedAt TIMESTAMP NOT NULL,          -- BaseEntity field
    CreatedBy VARCHAR(255) NOT NULL,       -- BaseEntity field
    UpdatedAt TIMESTAMP,                   -- BaseEntity field
    UpdatedBy VARCHAR(255),                -- BaseEntity field
    Version INT NOT NULL,                  -- BaseEntity field
    EntityData TEXT,                       -- BaseEntity field

    CONSTRAINT fk_registrant_spectator FOREIGN KEY (RegistrantId) REFERENCES Registrant(Id) ON DELETE CASCADE
);

-- 7. Create Indexes
CREATE INDEX idx_spectator_registrant_id ON Spectator(RegistrantId);

-- 8. Create Competitor Table
CREATE TABLE Competitor (
    Id UUID PRIMARY KEY, -- Unique identifier for each Competitor
    RegistrantId UUID NOT NULL, -- Foreign key referencing Registrant(Id)
    District INT NOT NULL, -- District number, required
    CompetitionStatus VARCHAR(50) NOT NULL DEFAULT 'Competing', -- Status with default value
    Quizzing BOOLEAN NOT NULL DEFAULT FALSE, -- Whether they are quizzing
    ArtCategories TEXT[], -- List of art categories, stored as array
    CreativeMinistriesCategories TEXT[], -- List of creative ministries categories, stored as array
    CreativeWritingCategories TEXT[], -- List of creative writing categories, stored as array
    SpeechCategories TEXT[], -- List of speech categories, stored as array
    AcademicCategories TEXT[], -- List of academic categories, stored as array
    VocalMusicCategories TEXT[], -- List of vocal music categories, stored as array
    InstrumentalMusicCategories TEXT[], -- List of instrumental music categories, stored as array
    IndividualSportCategories TEXT[], -- List of individual sport categories, stored as array
    TeamSportCategories TEXT[], -- List of team sport categories, stored as array
    AttendingTNTatTNU BOOLEAN NOT NULL, -- Whether they are attending TNT at TNU
    CONSTRAINT fk_registrant FOREIGN KEY (RegistrantId) REFERENCES Registrant(Id) ON DELETE CASCADE
);

CREATE INDEX idx_competitor_registrant_id ON Competitor(RegistrantId);

-- -- Competitor Table
-- CREATE TABLE Competitor (
--     Id UUID PRIMARY KEY,
--     RegistrantId UUID NOT NULL,
--     District INT NOT NULL,
--     CompetitionStatus VARCHAR(50) NOT NULL DEFAULT 'Competing',
--     Quizzing BOOLEAN NOT NULL DEFAULT FALSE,
--     AttendingTNTatTNU BOOLEAN NOT NULL,
--     CONSTRAINT fk_registrant FOREIGN KEY (RegistrantId) REFERENCES Registrant(Id) ON DELETE CASCADE
-- );

-- -- CompetitorCategories Table for storing categories
-- CREATE TABLE CompetitorCategories (
--     CompetitorId UUID NOT NULL,
--     CategoryType VARCHAR(50) NOT NULL,
--     CategoryName VARCHAR(255) NOT NULL,
--     CONSTRAINT fk_competitor FOREIGN KEY (CompetitorId) REFERENCES Competitor(Id) ON DELETE CASCADE
-- );



