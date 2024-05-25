CREATE TABLE IF NOT EXISTS church(
    id varchar(256) primary key,
    name varchar(256),
    address_street_address_1 varchar(256),
    address_street_address_2 varchar(256),
    address_locality varchar(256),
    address_postal_code varchar(256),
    address_country varchar(256),
    address_administrative_area_level_1 varchar(256)
);

CREATE INDEX ON church (name);