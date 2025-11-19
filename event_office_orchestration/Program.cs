using RoBrosEventsService.Data;
using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Services;
using RoBrosEventsService.Endpoints;

using EventOfficeApi.RoBrosAddressesService.Extensions;
using RoBrosRegistrantsService.Services;
using RoBrosRegistrantsService.Endpoints;
using RoBrosRegistrantsService.Controllers;
using RoBrosRegistrantsService.Data;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NSwag;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword;Port=5432;";
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

await using var dataSource = dataSourceBuilder.Build();
// This should inject a logger and anything else that comes from the consuming service
builder.Services.AddAddressPackage(dataSource);

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

// app.MapReservationEndpoints();
// app.MapEventEndpoints();
// app.MapRegistrantEndpoints();
// app.MapChurchEndpoints();


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();
