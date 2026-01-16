using EventOfficeApi.RoBrosAddressesService.Interfaces;

namespace EventOfficeApi.RoBrosAddressesService.Data;

public class SqlServerProvider : ISqlProvider
{
    public virtual string GetCreateAddressQuery()
    {
        return @"
        MERGE addresses AS target
        USING (SELECT @StreetAddress1 AS street_address_1, @City AS city, @PostalCode AS postal_code) AS source
        ON target.street_address_1 = source.street_address_1 
           AND target.city = source.city 
           AND target.postal_code = source.postal_code
        WHEN NOT MATCHED THEN
            INSERT (street_address_1, street_address_2, city, state, postal_code, country, created_at, created_by, updated_at, updated_by, version)
            VALUES (@StreetAddress1, @StreetAddress2, @City, @State, @PostalCode, @Country, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, 1)
        OUTPUT INSERTED.id;";
    }

    public virtual string GetUpdateAddressQuery()
    {
        return @"
            UPDATE addresses 
            SET street_address_1 = COALESCE(@StreetAddress1, street_address_1),
                street_address_2 = COALESCE(@StreetAddress2, street_address_2),
                city = COALESCE(@City, city),
                state = COALESCE(@State, state),
                postal_code = COALESCE(@PostalCode, postal_code),
                country = COALESCE(@Country, country),
                updated_at = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE id = @Id;";
    }

    public virtual string GetDeleteAddressQuery()
    {
        return "DELETE FROM addresses WHERE id = @Id;";
    }

    public virtual string GetSelectAddressByIdQuery()
    {
        return @"
            SELECT a.*, m.id as mapping_id, m.entity_id, m.entity_type, m.address_type, m.created_at as mapping_created_at
            FROM addresses a
            LEFT JOIN address_entity_mappings m ON a.id = m.address_id
            WHERE a.id = @Id;";
    }

    public virtual string GetSelectAddressByEntityQuery()
    {
        return @"
            SELECT a.*, m.id as mapping_id, m.entity_id, m.entity_type, m.address_type, m.created_at as mapping_created_at
            FROM addresses a
            INNER JOIN address_entity_mappings m ON a.id = m.address_id
            WHERE m.entity_id = @EntityId AND m.entity_type = @EntityType;";
    }

    public virtual string GetCreateMappingQuery()
    {
        return @"
            INSERT INTO address_entity_mappings (id, address_id, entity_id, entity_type, address_type, created_at)
            OUTPUT INSERTED.*
            VALUES (@Id, @AddressId, @EntityId, @EntityType, @AddressType, @CreatedAt);";
    }

    public virtual string GetDeleteMappingQuery()
    {
        return @"
            DELETE FROM address_entity_mappings 
            WHERE address_id = @AddressId AND entity_id = @EntityId AND entity_type = @EntityType;";
    }

    public virtual string GetSelectMappingsByEntityQuery()
    {
        return @"
            SELECT * FROM address_entity_mappings 
            WHERE entity_id = @EntityId AND entity_type = @EntityType;";
    }

    public virtual string GetSearchAddressesQuery()
    {
        return @"
            SELECT a.*, m.id as mapping_id, m.entity_id, m.entity_type, m.address_type, m.created_at as mapping_created_at
            FROM addresses a
            LEFT JOIN address_entity_mappings m ON a.id = m.address_id
            WHERE (@City IS NULL OR LOWER(a.city) LIKE LOWER(@City))
              AND (@State IS NULL OR LOWER(a.state) LIKE LOWER(@State))
              AND (@PostalCode IS NULL OR a.postal_code LIKE @PostalCode);";
    }

    public virtual string GetAddressExistsQuery()
    {
        return @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM addresses WHERE id = @Id
                UNION 
                SELECT 1 FROM addresses WHERE street_address_1 = @StreetAddress1 AND city = @City AND postal_code = @PostalCode
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;";
    }

    public virtual string GetCreateTablesScript()
    {
        return @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'addresses')
            BEGIN
                CREATE TABLE addresses (
                    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    street_address_1 NVARCHAR(255) NOT NULL,
                    street_address_2 NVARCHAR(255) NULL,
                    city NVARCHAR(100) NOT NULL,
                    state NVARCHAR(100) NOT NULL,
                    postal_code NVARCHAR(20) NOT NULL,
                    country NVARCHAR(100) NOT NULL DEFAULT 'USA',
                    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                    created_by NVARCHAR(100) NOT NULL,
                    updated_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                    updated_by NVARCHAR(100) NOT NULL,
                    version INT NOT NULL DEFAULT 1,
                    CONSTRAINT UQ_addresses_street_city_postal UNIQUE (street_address_1, city, postal_code)
                );
            END;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'address_entity_mappings')
            BEGIN
                CREATE TABLE address_entity_mappings (
                    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    address_id UNIQUEIDENTIFIER NOT NULL,
                    entity_id UNIQUEIDENTIFIER NOT NULL,
                    entity_type NVARCHAR(100) NOT NULL,
                    address_type NVARCHAR(50) NULL,
                    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                    CONSTRAINT FK_mappings_addresses FOREIGN KEY (address_id) 
                        REFERENCES addresses(id) ON DELETE CASCADE,
                    CONSTRAINT UQ_mappings_address_entity UNIQUE (address_id, entity_id, entity_type)
                );
            END;

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_addresses_city' AND object_id = OBJECT_ID('addresses'))
                CREATE INDEX idx_addresses_city ON addresses(city);

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_addresses_state' AND object_id = OBJECT_ID('addresses'))
                CREATE INDEX idx_addresses_state ON addresses(state);

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_addresses_postal_code' AND object_id = OBJECT_ID('addresses'))
                CREATE INDEX idx_addresses_postal_code ON addresses(postal_code);

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mappings_entity' AND object_id = OBJECT_ID('address_entity_mappings'))
                CREATE INDEX idx_mappings_entity ON address_entity_mappings(entity_id, entity_type);

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mappings_address' AND object_id = OBJECT_ID('address_entity_mappings'))
                CREATE INDEX idx_mappings_address ON address_entity_mappings(address_id);
        ";
    }
}