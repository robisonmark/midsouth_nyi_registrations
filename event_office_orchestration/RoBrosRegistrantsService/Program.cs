using Microsoft.OpenApi.Models;
<<<<<<< HEAD
using Microsoft.Extensions.DependencyInjection;
=======
>>>>>>> aaadf4e (Feature/addresses service (#8))
using Npgsql;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// Register NpgsqlConnection as a singleton
builder.Services.AddSingleton<NpgsqlConnection>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    var connection = new NpgsqlConnection(connectionString);
    connection.Open(); // Open the connection when the singleton is created
    return connection;
});

=======
>>>>>>> aaadf4e (Feature/addresses service (#8))
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new NSwag.OpenApiInfo
        {
            Title = "Event Office Registants Service",
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

<<<<<<< HEAD
=======
// Register DbContext with Npgsql connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

>>>>>>> aaadf4e (Feature/addresses service (#8))
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add OpenAPI 3.0 document serving middleware
    // Available at: http://localhost:<port>/swagger/v1/swagger.json
    app.UseOpenApi();

    // Add web UIs to interact with the document
    // Available at: http://localhost:<port>/swagger
    app.UseSwaggerUi(); // UseSwaggerUI Protected by if (env.IsDevelopment())

    // Add ReDoc UI to interact with the document
    // Available at: http://localhost:<port>/redoc
    app.UseReDoc(settings =>
    {
        settings.DocumentTitle = "Registrants Service";
        settings.Path = "/redoc";
    });
}

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();
