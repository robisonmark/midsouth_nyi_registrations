using Microsoft.AspNetCore.Mvc;
using YourCompany.AddressService.Services;
using YourCompany.AddressService.Models;

namespace AddressService.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAddress(CreateAddressRequest request)
    {
        var addressId = await _addressService.CreateAddressAsync(request);
        return Ok(new { AddressId = addressId });
    }
    
    [HttpPost("{addressId}/link")]
    public async Task<IActionResult> LinkToEntity(int addressId, EntityInfo entityInfo)
    {
        var linkedAddressId = await _addressService.LinkAddressToEntityAsync(addressId, entityInfo);
        return Ok(new { AddressId = linkedAddressId });
    }
    
    [HttpGet("{addressId}")]
    public async Task<IActionResult> GetAddress(int addressId)
    {
        var address = await _addressService.GetAddressByIdAsync(addressId);
        if (address == null) return NotFound();
        return Ok(address);
    }
    
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetEntityAddresses(string entityType, int entityId)
    {
        var addresses = await _addressService.GetEntityAddressesAsync(entityType, entityId);
        return Ok(addresses);
    }
    
    [HttpPost("search")]
    public async Task<IActionResult> SearchAddresses(SearchAddressCriteria criteria, [FromQuery] SearchOptions? options = null)
    {
        var addresses = await _addressService.SearchAddressesAsync(criteria, options);
        return Ok(addresses);
    }
}