using EventOfficeApi.RoBrosAddressesService.Interfaces;
using EventOfficeApi.RoBrosAddressesService.Models;
using Microsoft.Extensions.Logging;


namespace EventOfficeApi.RoBrosAddressesService.Services;

public interface IAddressService
{
    // Task InitializeAsync(); // I think this is to initialize DB schema, etc.
    // Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null);
    // Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo);
    // Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null);
    // Task<Address?> GetAddressByIdAsync(int addressId);
    // Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData);
    // Task<bool> DeactivateAddressAsync(int addressId);
    // Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null);
    // Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null);

    Task<Address?> GetAddressAsync(Guid id);
    Task<IEnumerable<Address>> GetAddressesByEntityAsync(Guid entityId, string entityType);
    Task<Address> CreateAddressAsync(CreateAddressRequest request);
    Task<Address?> UpdateAddressAsync(Guid id, UpdateAddressRequest request);
    Task<bool> DeleteAddressAsync(Guid id);
    Task<bool> AssignAddressToEntityAsync(Guid addressId, Guid entityId, string entityType, string? addressType = null);
    Task<bool> RemoveAddressFromEntityAsync(Guid addressId, Guid entityId, string entityType);
    Task<IEnumerable<Address>> SearchAddressesAsync(string? city = null, string? state = null, string? postalCode = null);
    Task<bool> ValidateAddressAsync(Guid id);
}