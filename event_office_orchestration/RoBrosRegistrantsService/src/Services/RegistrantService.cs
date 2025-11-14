// ASP.NET Libraries
using Microsoft.Extensions.Logging;

// RoBros Libraries
using EventOfficeApi.RoBrosAddressesService.Models;
using EventOfficeApi.RoBrosAddressesService.Services;

// Local
using RoBrosRegistrantsService.Data;
using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;

// Address Service Reference
// using EventOfficeApi.RoBrosAddressesService.Extensions;
// using EventOfficeApi.RoBrosAddressesService.Models;

namespace RoBrosRegistrantsService.Services;

public interface IRegistrantService
{
    Task<Guid> CreateRegistrantAsync(Registrant registrant);
    Task<Registrant> GetRegistrantAsync(Guid id);
    // Task<IEnumerable<Registrant>> GetAllRegistrantsAsync();
    // Task<Registrant?> UpdateRegistrantAsync(Registrant registrant); // TODO: Break this into smaller parts and a whole, should it be named put
    Task<IEnumerable<Registrant>> SearchRegistrantsAsync(string searchParameters);
}

public class RegistrantService: IRegistrantService
{
    private readonly IAddressService _addressService;
    private readonly IChurchService _churchService;
    private readonly IRegistrantRepository _repository;
    private readonly ILogger<RegistrantService> _logger;
    

    public RegistrantService(
        IRegistrantRepository repository,
        IAddressService addressService,
        IChurchService churchService,
        ILogger<RegistrantService> logger)
    {
        _addressService = addressService;
        _churchService = churchService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CreateRegistrantAsync(Registrant registrant)
    {
        _logger.LogInformation("Creating a new registrant");

        Guid church_id = await _churchService.CreateChurchAsync(registrant.Church);
        registrant.ChurchId = church_id;
        
        Guid address_id = await _addressService.CreateAddressAsync(registrant.Address);
        registrant.AddressId = address_id;

        Console.WriteLine(registrant.GetType().ToString());

        if (registrant.CompetitionStatus.ToLower() == "competing")
        {
            Console.WriteLine("Registrant is a competitor.");
        }
        
        return await _repository.CreateRegistrantAsync(registrant);
    }

    public async Task<Registrant> GetRegistrantAsync(Guid id)
    {
        _logger.LogInformation("Get existing registrant");
        
        return await _repository.GetRegistrantAsync(id);
    }

    public async Task<IEnumerable<Registrant>> SearchRegistrantsAsync(String searchParameters)
    {
        _logger.LogInformation("Search registrants");

        return await _repository.SearchRegistrantsAsync(searchParameters);
    }

}