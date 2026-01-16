using EventOfficeApi.RoBrosAddressesService.Models;

namespace EventOfficeApi.RoBrosAddressesService.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id);
    Task<IEnumerable<Address>> GetByEntityAsync(Guid entityId, string entityType);
    Task<Guid> CreateAsync(CreateAddressRequest request);
    Task<Address?> UpdateAsync(Guid id, UpdateAddressRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<AddressEntityMapping> MapToEntityAsync(Guid addressId, Guid entityId, string entityType, string? addressType = null);
    Task<bool> UnmapFromEntityAsync(Guid addressId, Guid entityId, string entityType);
    Task<IEnumerable<AddressEntityMapping>> GetMappingsByEntityAsync(Guid entityId, string entityType);
    Task<IEnumerable<Address>> SearchAsync(string? city = null, string? state = null, string? postalCode = null);
    Task<bool> ExistsAsync(Guid id);
}

public interface ISqlProvider
{
    string GetCreateAddressQuery();
    string GetUpdateAddressQuery();
    string GetDeleteAddressQuery();
    string GetSelectAddressByIdQuery();
    string GetSelectAddressByEntityQuery();
    string GetCreateMappingQuery();
    string GetDeleteMappingQuery();
    string GetSelectMappingsByEntityQuery();
    string GetSearchAddressesQuery();
    string GetAddressExistsQuery();
}