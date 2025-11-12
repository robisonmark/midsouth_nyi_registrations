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
    private readonly ChurchRepository _churchRepository;

    public ChurchService(ChurchRepository churchRepository)
    {
        _churchRepository = churchRepository;
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
