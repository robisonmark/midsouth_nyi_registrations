using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the address service for isolated testing
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "Test_Addresses";
        options.JoinTableName = "Test_EntityAddresses";
        options.AllowSchemaOverride = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize the schema on startup
using (var scope = app.Services.CreateScope())
{
    var addressService = scope.ServiceProvider.GetRequiredService<IAddressService>();
    await addressService.InitializeAsync();
}

app.Run();