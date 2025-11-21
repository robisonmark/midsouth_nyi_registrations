using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using NSwag;
// RoBros Libraries
using EventOfficeApi.RoBrosAddressesService.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "";

// Register application services
// I Need to understand this better. ChatGPT explains
builder.Services.AddScoped<RoBrosRegistrantsService.Services.DatabaseService>();
builder.Services.AddScoped<RoBrosRegistrantsService.Services.IRegistrantService, RoBrosRegistrantsService.Services.RegistrantService>();
builder.Services.AddScoped<RoBrosRegistrantsService.Data.IRegistrantRepository, RoBrosRegistrantsService.Data.RegistrantRepository>();
builder.Services.AddScoped<RoBrosRegistrantsService.Services.IChurchService, RoBrosRegistrantsService.Services.ChurchService>();
builder.Services.AddScoped<RoBrosRegistrantsService.Data.IChurchRepository, RoBrosRegistrantsService.Data.ChurchRepository>();

builder.Services.AddAddressPackage(connectionString);

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
            Title = "Event Office Registrants Service",
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

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    // Add OpenAPI document serving middleware
    app.UseOpenApi();
    // Add web UI for Swagger
    app.UseSwaggerUi(); // Use Swagger UI (from NSwag)
    // Optionally add ReDoc for another UI
    app.UseReDoc(settings =>
    {
        settings.DocumentTitle = "Registrants Service";
        settings.Path = "/redoc";
        settings.DocumentPath = "/swagger/v1/swagger.json"; // Set the correct Swagger document path
    });
// }

// Configure the routes
app.MapControllers();

app.Run();