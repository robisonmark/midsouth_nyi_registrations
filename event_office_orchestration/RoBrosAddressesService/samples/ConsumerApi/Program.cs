using EventOfficeApi.RoBrosAddressesService.Extensions;
using EventOfficeApi.RoBrosAddressesService.Models;
using EventOfficeApi.RoBrosAddressesService.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Address Package with default PostgreSQL provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=addressdev;Username=devuser;Password=devpass";

builder.Services.AddAddressPackage(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Minimal API endpoints for address operations
app.MapGet("/addresses/{id:guid}", async (Guid id, IAddressService addressService) =>
{
    var address = await addressService.GetAddressAsync(id);
    return address is not null ? Results.Ok(address) : Results.NotFound();
})
.WithName("GetAddress")
.WithOpenApi();

app.MapPost("/addresses", async ([FromBody] CreateAddressRequest request, IAddressService addressService) =>
{
    try
    {
        var address = await addressService.CreateAddressAsync(request);
        return Results.Created($"/addresses/{address.Id}", address);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("CreateAddress")
.WithOpenApi();

app.MapPut("/addresses/{id:guid}", async (Guid id, [FromBody] UpdateAddressRequest request, IAddressService addressService) =>
{
    var address = await addressService.UpdateAddressAsync(id, request);
    return address is not null ? Results.Ok(address) : Results.NotFound();
})
.WithName("UpdateAddress")
.WithOpenApi();

app.MapDelete("/addresses/{id:guid}", async (Guid id, IAddressService addressService) =>
{
    var deleted = await addressService.DeleteAddressAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteAddress")
.WithOpenApi();

app.MapGet("/entities/{entityId:guid}/addresses", async (Guid entityId, string entityType, IAddressService addressService) =>
{
    var addresses = await addressService.GetAddressesByEntityAsync(entityId, entityType);
    return Results.Ok(addresses);
})
.WithName("GetAddressesByEntity")
.WithOpenApi();

app.MapPost("/addresses/{addressId:guid}/assign", async (
    Guid addressId, 
    Guid entityId, 
    string entityType, 
    string? addressType,
    IAddressService addressService) =>
{
    var success = await addressService.AssignAddressToEntityAsync(addressId, entityId, entityType, addressType);
    return success ? Results.Ok() : Results.BadRequest("Failed to assign address to entity");
})
.WithName("AssignAddressToEntity")
.WithOpenApi();

app.MapDelete("/addresses/{addressId:guid}/unassign", async (
    Guid addressId, 
    Guid entityId, 
    string entityType,
    IAddressService addressService) =>
{
    var success = await addressService.RemoveAddressFromEntityAsync(addressId, entityId, entityType);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UnassignAddressFromEntity")
.WithOpenApi();

app.MapGet("/addresses/search", async (
    string? city, 
    string? state, 
    string? postalCode,
    IAddressService addressService) =>
{
    var addresses = await addressService.SearchAddressesAsync(city, state, postalCode);
    return Results.Ok(addresses);
})
.WithName("SearchAddresses")
.WithOpenApi();

app.Run();