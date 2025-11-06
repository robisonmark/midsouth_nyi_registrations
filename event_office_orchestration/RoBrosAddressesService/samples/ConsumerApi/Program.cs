using EventOfficeApi.RoBrosAddressesService.Extensions;
using EventOfficeApi.RoBrosAddressesService.Models;
using EventOfficeApi.RoBrosAddressesService.Services;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Address Package with default PostgreSQL provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=RoBrosAddresses;Username=postgres;Password=YourPassword;Timeout=30;";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
// using var dataSource = NpgsqlDataSource.Create(connectionString);
var dataSource = dataSourceBuilder.Build();

// This should inject a logger and anything else that comes from the consuming service
builder.Services.AddAddressPackage(dataSource);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Register OpenAPI (NSwag) document configuration
builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new NSwag.OpenApiInfo
        {
            Title = "Event Office Addresses Service",
            Description = "API for Defining The Students and Chaperones registering for an Event",
            Version = "v1",
            Contact = new NSwag.OpenApiContact()
            {
                Name = "Mark Robison",
                Email = "admin@robros.tech",
                Url = "https://robros.tech"
            },
        };
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSwaggerUi(); // Use Swagger UI (from NSwag)

    // Optionally add ReDoc for another UI
    app.UseReDoc(settings =>
    {
        settings.DocumentTitle = "Addresses Service";
        settings.Path = "/redoc";
        settings.DocumentPath = "/swagger/v1/swagger.json"; // Set the correct Swagger document path
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Minimal API endpoints for address operations
app.MapGet("/address/{id:guid}", async (Guid id, IAddressService addressService) =>
{
    Console.WriteLine("---------- Grabbing Address ---------------");
    var address = await addressService.GetAddressAsync(id);
    return address is not null ? Results.Ok(address) : Results.NotFound();
})
.WithName("GetAddress")
.WithOpenApi();

app.MapPost("/addresses", async ([FromBody] CreateAddressRequest request, IAddressService addressService) =>
{
    try
    {
        Console.WriteLine("----- Creating address... -----");
        var address = await addressService.CreateAddressAsync(request);
        return Results.Created($"/addresses/{address.Id}", address);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Argument Exception: {ex.Message}");
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