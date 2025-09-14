-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create addresses table
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

-- Create address entity mappings table
CREATE TABLE IF NOT EXISTS address_entity_mappings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    address_id UUID NOT NULL REFERENCES addresses(id) ON DELETE CASCADE,
    entity_id UUID NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    address_type VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(address_id, entity_id, entity_type)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_addresses_city ON addresses(city);
CREATE INDEX IF NOT EXISTS idx_addresses_state ON addresses(state);
CREATE INDEX IF NOT EXISTS idx_addresses_postal_code ON addresses(postal_code);
CREATE INDEX IF NOT EXISTS idx_addresses_created_at ON addresses(created_at);

CREATE INDEX IF NOT EXISTS idx_mappings_entity ON address_entity_mappings(entity_id, entity_type);
CREATE INDEX IF NOT EXISTS idx_mappings_address ON address_entity_mappings(address_id);
CREATE INDEX IF NOT EXISTS idx_mappings_address_type ON address_entity_mappings(address_type);

-- Insert sample data for development
INSERT INTO addresses (id, street, city, state, postal_code, country) VALUES
    ('550e8400-e29b-41d4-a716-446655440001', '123 Main Street', 'Nashville', 'TN', '37203', 'USA'),
    ('550e8400-e29b-41d4-a716-446655440002', '456 Oak Avenue', 'Memphis', 'TN', '38103', 'USA'),
    ('550e8400-e29b-41d4-a716-446655440003', '789 Pine Road', 'Knoxville', 'TN', '37901', 'USA')
ON CONFLICT (id) DO NOTHING;