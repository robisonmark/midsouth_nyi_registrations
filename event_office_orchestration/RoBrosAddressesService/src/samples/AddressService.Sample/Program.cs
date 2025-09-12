using Microsoft.Data.SqlClient;
using Npgsql;
using NSwag;
using EventOfficeApi.AddressService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the address service for isolated testing
builder.Services.AddAddressService(
    serviceProvider => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "Test_Addresses";
        options.JoinTableName = "Test_EntityAddresses";
        options.AllowSchemaOverride = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseReDoc(settings =>
    {
        settings.DocumentTitle = "Address Service";
        settings.Path = "/redoc";
        settings.DocumentPath = "/swagger/v1/swagger.json"; // Set the correct Swagger document path
    });
}

app.MapControllers();

// Initialize the schema on startup
using (var scope = app.Services.CreateScope())
{
    var addressService = scope.ServiceProvider.GetRequiredService<IAddressService>();
    await addressService.InitializeAsync();
}

app.Run();