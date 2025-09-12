using EventOfficeApi.AddressService.Models;

namespace EventOfficeApi.AddressService.Services;

public interface IAddressService
{
    Task InitializeAsync();
    Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null);
    Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo);
    Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null);
    Task<Address?> GetAddressByIdAsync(int addressId);
    Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData);
    Task<bool> DeactivateAddressAsync(int addressId);
    Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null);
    Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null);
    Task<AddressStats> GetStatsAsync();
}