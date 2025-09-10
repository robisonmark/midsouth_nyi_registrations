public class AddressService : IAddressService
{
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
}