using EventOfficeApi.RoBrosAddressesService.Interfaces;
using EventOfficeApi.RoBrosAddressesService.Models;
using Microsoft.Extensions.Logging;


namespace EventOfficeApi.RoBrosAddressesService.Services;

public interface IAddressService
{
    Task<Address?> GetAddressAsync(Guid id);
    Task<IEnumerable<Address>> GetAddressesByEntityAsync(Guid entityId, string entityType);
    Task<Guid> CreateAddressAsync(CreateAddressRequest request);
    Task<Address?> UpdateAddressAsync(Guid id, UpdateAddressRequest request);
    Task<bool> DeleteAddressAsync(Guid id);
    Task<bool> AssignAddressToEntityAsync(Guid addressId, Guid entityId, string entityType, string? addressType = null);
    Task<bool> RemoveAddressFromEntityAsync(Guid addressId, Guid entityId, string entityType);
    Task<IEnumerable<Address>> SearchAddressesAsync(string? city = null, string? state = null, string? postalCode = null);
    Task<bool> ValidateAddressAsync(Guid id);
}