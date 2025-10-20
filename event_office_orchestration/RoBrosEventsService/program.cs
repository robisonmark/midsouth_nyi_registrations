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
using Npgsql;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Register dependencies
// builder.Services.AddScoped<ISqlProvider, PostgresProvider>();
// builder.Services.AddScoped<IEventRepository, EventRepository>();
// builder.Services.AddScoped<EventService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
//     ?? ;

var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword;Port=5432;";
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
Console.WriteLine($"DB ConnectionString: {connectionString}");

// var dataSource = dataSourceBuilder.Build();
await using var dataSource = dataSourceBuilder.Build();
// This should inject a logger and anything else that comes from the consuming service
builder.Services.AddEventPackage(dataSource);

// builder.Services.AddOpenApiDocument(options =>
// {
//     options.PostProcess = document =>
//     {
//         document.Info = new NSwag.OpenApiInfo
//         {
//             Title = "Event Office Addresses Service",
//             Description = "API for Defining The Students and Chaperones registering for an Event",
//             Version = "v1",
//             Contact = new NSwag.OpenApiContact()
//             {
//                 Name = "Mark Robison",
//                 Email = "admin@robros.tech",
//                 Url = "https://robros.tech"
//             },
//         };
//     };
// });

var app = builder.Build();

app.MapEventEndpoints();

// if (app.Environment.IsDevelopment())
// {
    // app.UseSwagger();
    // app.UseSwaggerUI();
// }

app.Run();
