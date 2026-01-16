using RoBrosEventsService.Data;
using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Services;
using RoBrosEventsService.Endpoints;
using RoBrosEventsService.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Register dependencies
// builder.Services.AddScoped<ISqlProvider, SqlServerProvider>();
// builder.Services.AddScoped<IEventRepository, EventRepository>();
// builder.Services.AddScoped<EventService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword;Port=5432;";
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

Console.WriteLine($"DB ConnectionString: {connectionString}");

// SQL Server doesn't use a data source builder like Npgsql
// Pass the connection string directly to AddEventPackage
// This now registers EventService, IEventRepository, and ISqlProvider
builder.Services.AddEventPackage(connectionString);

builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new NSwag.OpenApiInfo
        {
            Title = "Event Office Event Service",
            Description = "API for Defining The Students and Chaperones registering for an Event",
            Version = "v1",
            Contact = new NSwag.OpenApiContact()
            {
                Name = "Mark Robison",
                Email = "admin@robros.tech",
                Url = "https://robros.dev"
            },
        };
    };
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll",
//         builder =>
//         {
//             builder.AllowAnyOrigin()
//                   .AllowAnyHeader()
//                   .AllowAnyMethod();
//         });
// });

var app = builder.Build();

app.MapReservationEndpoints();
app.MapEventEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();