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
            Title = "Event Office Addresses Service",
            Description = "API Used to Create and Fetch Address for Churches, Event Location, and Registrants",
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

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {

//     options.SwaggerDoc("v1",
//         new Microsoft.OpenApi.Models.OpenApiInfo
//         {
//     Title = "Event Office Addresses Service",
//     Description = "API Used to Create and Fetch Address for Churches, Event Location, and Registrants",
//     Version = "v1",
//     Contact = new Microsoft.OpenApi.Models.OpenApiContact()
//     {
//         Name = "Mark Robison",
//         Email = "admin@robros.tech",
//         Url = new Uri("https://robros.tech")
//     },
//     Extensions = new Dictionary<string, IOpenApiExtension>
//     {
//         {"x-logo", new OpenApiObject
//             {
//                  { "url", new OpenApiString("https://foforks.com.br/wp-content/uploads/2019/05/news.jpeg") },
//                  { "altText", new OpenApiString("Replace with real logo") }
//             }
//         }
//     }
// });
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    // {
    //     options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    //     options.RoutePrefix = string.Empty;
    // });

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
        settings.DocumentTitle = "Addresses Service";
        settings.Path = "/redoc";
    });
}

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();
