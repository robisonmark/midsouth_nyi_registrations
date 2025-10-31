using Microsoft.Extensions.Logging;
using RoBrosRegistrantsService.Data;
using RoBrosRegistrantsService.Models;

// Address Service Reference
// using EventOfficeApi.RoBrosAddressesService.Extensions;
// using EventOfficeApi.RoBrosAddressesService.Models;
// using EventOfficeApi.RoBrosAddressesService.Services;

namespace RoBrosRegistrantsService.Services;

public interface IRegistrantService
{
    Task<Guid> CreateRegistrantAsync(Registrant registrant);
    Task<Registrant> GetRegistrantAsync(Guid id);
    // Task<IEnumerable<Registrant>> GetAllRegistrantsAsync();
    // Task<Registrant?> UpdateRegistrantAsync(Registrant registrant); // TODO: Break this into smaller parts and a whole, should it be named put
    // Task<Registrants?> SearchRegistrantsAsync(string queryStringParameters);
}

public class RegistrantService: IRegistrantService
{
    private readonly IRegistrantRepository _repository;
    private readonly ILogger<RegistrantService> _logger;

    public RegistrantService(IRegistrantRepository repository, ILogger<RegistrantService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CreateRegistrantAsync(Registrant registrant)
    {
        _logger.LogInformation("Creating a new registrant");

        if (registrant.Address == null)
        {
            // TODO: Check for Address in Address Service before creating a new one
            registrant.Address = new Address();
        }
        if (registrant.Church.Id == null)
        {
            // TODO: Check for Address in Address Service before creating a new one
            registrant.ChurchId = Guid.NewGuid();
        }
        
        return await _repository.CreateRegistrantAsync(registrant);
    }

    public async Task<Registrant> GetRegistrantAsync(Guid id)
    {
        _logger.LogInformation("Get existing registrant");
        
        return await _repository.GetRegistrantAsync(id);
    }

    public async Task<IEnumerable<Registrant>> SearchRegistrantsAsync(String queryStringParameters)
    {
        _logger.LogInformation("Search registrants");
        
        return await _repository.SearchRegistrantsAsync(queryStringParameters);
    }

}