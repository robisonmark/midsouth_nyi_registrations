// ASP.NET Libraries
using Microsoft.Extensions.Logging;

// RoBros Libraries
using EventOfficeApi.RoBrosAddressesService.Models;
using EventOfficeApi.RoBrosAddressesService.Services;

using RoBrosRegistrantsService.Data;
using RoBrosRegistrantsService.Models;

namespace RoBrosRegistrantsService.Services;

public interface IChurchService
{
    Task<Guid> CreateChurchAsync(Church church);
    Task<Church?> GetChurchAsync(Guid id);
    // Task<IEnumerable<Registrant>> GetAllRegistrantsAsync();
    // Task<Registrant?> UpdateRegistrantAsync(Registrant registrant); // TODO: Break this into smaller parts and a whole, should it be named put
    // Task<Registrants?> SearchRegistrantsAsync(string queryStringParameters);
}

public class ChurchService : IChurchService
{
    private readonly IChurchRepository _churchRepository;
    private readonly IAddressService _addressService;
    private readonly ILogger<ChurchService> _logger;

    public ChurchService(
        IChurchRepository churchRepository,
        IAddressService addressService,
        ILogger<ChurchService> logger
    )
    {
        _churchRepository = churchRepository;
        _addressService = addressService;
        _logger = logger;
    }

    public async Task<Guid> CreateChurchAsync(Church church)
    {
        if (!string.IsNullOrWhiteSpace(church.Name))
        {
            var existing = await _churchRepository.GetByNameAsync(church.Name);
            if (existing != null && existing.Id != null)
            {
                return existing.Id.Value;
            }
        }

        if (church.Address != null)
        {
            Guid addressId = await _addressService.CreateAddressAsync(church.Address);
            church.AddressId = addressId;
        }
        
        // Ensure ID and audit fields
        church.Id ??= Guid.NewGuid();
        church.createdBy ??= "System";
        church.updatedBy ??= "System";

        // If a church with the same name already exists, return its id instead of creating a duplicate

        var id = await _churchRepository.CreateAsync(church);
        return id;
    }

    public async Task<Church?> GetChurchAsync(Guid id)
    {
        return await _churchRepository.GetByIdAsync(id);
    }
}
