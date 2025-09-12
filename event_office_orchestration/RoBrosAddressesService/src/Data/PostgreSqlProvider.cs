using EventOfficeApi.AddressService.Interfaces;

namespace EventOfficeApi.AddressService.Data;

public class PostgreSqlProvider : ISqlProvider
{
    public virtual string GetCreateAddressQuery()
    {
        return @"
            INSERT INTO addresses (id, street, street2, city, state, postal_code, country, created_at, updated_at)
            VALUES (@Id, @Street, @Street2, @City, @State, @PostalCode, @Country, @CreatedAt, @UpdatedAt)
            RETURNING *;";
    }

    public virtual string GetUpdateAddressQuery()
    {
        return @"
            UPDATE addresses 
            SET street = COALESCE(@Street, street),
                street2 = COALESCE(@Street2, street2),
                city = COALESCE(@City, city),
                state = COALESCE(@State, state),
                postal_code = COALESCE(@PostalCode, postal_code),
                country = COALESCE(@Country, country),
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING *;";
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
            VALUES (@Id, @AddressId, @EntityId, @EntityType, @AddressType, @CreatedAt)
            RETURNING *;";
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
        return "SELECT EXISTS(SELECT 1 FROM addresses WHERE id = @Id);";
    }

    public virtual string GetCreateTablesScript()
    {
        return @"
            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";

            CREATE TABLE IF NOT EXISTS addresses (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                street VARCHAR(255) NOT NULL,
                street2 VARCHAR(255),
                city VARCHAR(100) NOT NULL,
                state VARCHAR(100) NOT NULL,
                postal_code VARCHAR(20) NOT NULL,
                country VARCHAR(100) NOT NULL DEFAULT 'USA',
                created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
                updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS address_entity_mappings (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                address_id UUID NOT NULL REFERENCES addresses(id) ON DELETE CASCADE,
                entity_id UUID NOT NULL,
                entity_type VARCHAR(100) NOT NULL,
                address_type VARCHAR(50),
                created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
                UNIQUE(address_id, entity_id, entity_type)
            );

            CREATE INDEX IF NOT EXISTS idx_addresses_city ON addresses(city);
            CREATE INDEX IF NOT EXISTS idx_addresses_state ON addresses(state);
            CREATE INDEX IF NOT EXISTS idx_addresses_postal_code ON addresses(postal_code);
            CREATE INDEX IF NOT EXISTS idx_mappings_entity ON address_entity_mappings(entity_id, entity_type);
            CREATE INDEX IF NOT EXISTS idx_mappings_address ON address_entity_mappings(address_id);
        ";
    }
}