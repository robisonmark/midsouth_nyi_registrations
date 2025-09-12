namespace EventOfficeApi.AddressService.Services;

public class AddressService : IAddressService
{

    private readonly IAddressRepository _repository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository repository, ILogger<AddressService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 🏗️ Schema Management Logic
    private async Task CreateSchemaAsync() { /* Schema creation logic */ }
    private async Task<bool> ValidateSchemaAsync() { /* Schema validation logic */ }

    // 📝 Address Business Logic
    public async Task<int> CreateAddressAsync(CreateAddressRequest request, EntityInfo? entityInfo = null)
    {
        // Validation logic
        await ValidateAddressRequest(request);

        // Normalization logic  
        var normalizedAddress = await NormalizeAddress(request);

        // Duplicate detection logic
        var existingId = await CheckForDuplicate(normalizedAddress);
        if (existingId.HasValue) return existingId.Value;

        // Creation logic
        return await CreateNewAddress(normalizedAddress, entityInfo);
    }

    // 🔍 Address Validation Logic
    private async Task ValidateAddressRequest(CreateAddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PostalCode))
            throw new AddressServiceException("Postal code is required");

        if (!await IsValidPostalCodeFormat(request.PostalCode, request.Country))
            throw new AddressServiceException("Invalid postal code format");

        // Add more validation rules
    }

    // 🧹 Address Normalization Logic
    private async Task<CreateAddressRequest> NormalizeAddress(CreateAddressRequest request)
    {
        return new CreateAddressRequest
        {
            StreetAddress = NormalizeStreetAddress(request.StreetAddress),
            City = NormalizeCity(request.City),
            State = NormalizeState(request.State, request.Country),
            PostalCode = NormalizePostalCode(request.PostalCode, request.Country),
            Country = request.Country?.ToUpper() ?? "US"
        };
    }

    // 🔄 Duplicate Detection Logic
    private async Task<int?> CheckForDuplicate(CreateAddressRequest normalized)
    {
        // Your duplicate detection algorithm
        var sql = $@"
            SELECT TOP 1 Id FROM [{_options.AddressTableName}]
            WHERE StreetAddress = @StreetAddress 
              AND City = @City 
              AND State = @State 
              AND PostalCode = @PostalCode
              AND Country = @Country
              AND IsActive = 1";

        var results = await QueryAsync<int?>(sql, normalized);
        return results.FirstOrDefault();
    }

    // ... all other address logic methods
    

    public async Task<Address?> GetAddressAsync(Guid id)
    {
        _logger.LogInformation("Getting address with ID: {AddressId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Address>> GetAddressesByEntityAsync(Guid entityId, string entityType)
    {
        _logger.LogInformation("Getting addresses for entity: {EntityId} of type: {EntityType}", entityId, entityType);
        return await _repository.GetByEntityAsync(entityId, entityType);
    }

    public async Task<Address> CreateAddressAsync(CreateAddressRequest request)
    {
        _logger.LogInformation("Creating new address for city: {City}, state: {State}", request.City, request.State);
        
        ValidateCreateRequest(request);
        
        var address = await _repository.CreateAsync(request);
        _logger.LogInformation("Created address with ID: {AddressId}", address.Id);
        
        return address;
    }

    public async Task<Address?> UpdateAddressAsync(Guid id, UpdateAddressRequest request)
    {
        _logger.LogInformation("Updating address with ID: {AddressId}", id);
        
        var existingAddress = await _repository.GetByIdAsync(id);
        if (existingAddress == null)
        {
            _logger.LogWarning("Address not found for update: {AddressId}", id);
            return null;
        }

        var updatedAddress = await _repository.UpdateAsync(id, request);
        if (updatedAddress != null)
        {
            _logger.LogInformation("Successfully updated address: {AddressId}", id);
        }

        return updatedAddress;
    }

    public async Task<bool> DeleteAddressAsync(Guid id)
    {
        _logger.LogInformation("Deleting address with ID: {AddressId}", id);
        
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Successfully deleted address: {AddressId}", id);
        }
        else
        {
            _logger.LogWarning("Address not found for deletion: {AddressId}", id);
        }

        return deleted;
    }

    public async Task<bool> AssignAddressToEntityAsync(Guid addressId, Guid entityId, string entityType, string? addressType = null)
    {
        _logger.LogInformation("Assigning address {AddressId} to entity {EntityId} of type {EntityType}", 
            addressId, entityId, entityType);

        try
        {
            // Verify address exists
            var addressExists = await _repository.ExistsAsync(addressId);
            if (!addressExists)
            {
                _logger.LogWarning("Cannot assign non-existent address: {AddressId}", addressId);
                return false;
            }

            await _repository.MapToEntityAsync(addressId, entityId, entityType, addressType);
            _logger.LogInformation("Successfully assigned address {AddressId} to entity {EntityId}", addressId, entityId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning address {AddressId} to entity {EntityId}: {Error}", 
                addressId, entityId, ex.Message);
            return false;
        }
    }

    public async Task<bool> RemoveAddressFromEntityAsync(Guid addressId, Guid entityId, string entityType)
    {
        _logger.LogInformation("Removing address {AddressId} from entity {EntityId} of type {EntityType}", 
            addressId, entityId, entityType);

        var removed = await _repository.UnmapFromEntityAsync(addressId, entityId, entityType);
        if (removed)
        {
            _logger.LogInformation("Successfully removed address mapping: {AddressId} -> {EntityId}", addressId, entityId);
        }
        else
        {
            _logger.LogWarning("Address mapping not found: {AddressId} -> {EntityId}", addressId, entityId);
        }

        return removed;
    }

    public async Task<IEnumerable<Address>> SearchAddressesAsync(string? city = null, string? state = null, string? postalCode = null)
    {
        _logger.LogInformation("Searching addresses with city: {City}, state: {State}, postal: {PostalCode}", 
            city, state, postalCode);

        return await _repository.SearchAsync(city, state, postalCode);
    }

    public async Task<bool> ValidateAddressAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    private static void ValidateCreateRequest(CreateAddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Street))
            throw new ArgumentException("Street is required", nameof(request.Street));

        if (string.IsNullOrWhiteSpace(request.City))
            throw new ArgumentException("City is required", nameof(request.City));

        if (string.IsNullOrWhiteSpace(request.State))
            throw new ArgumentException("State is required", nameof(request.State));

        if (string.IsNullOrWhiteSpace(request.PostalCode))
            throw new ArgumentException("Postal code is required", nameof(request.PostalCode));

        if (string.IsNullOrWhiteSpace(request.Country))
            throw new ArgumentException("Country is required", nameof(request.Country));
    }
}