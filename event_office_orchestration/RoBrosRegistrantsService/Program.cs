using Microsoft.OpenApi.Models;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    app.UseReDoc(settings =>
    {
        settings.DocumentTitle = "Registrants Service";
        settings.Path = "/redoc";
    });
}

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();
