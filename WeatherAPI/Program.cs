using WeatherAPI.Services;
using WeatherAPI.Settings;
using Microsoft.OpenApi.Models;
using WeatherAPI.Repositories;
using Microsoft.Extensions.Options;
using SharpCompress.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Find the filepath of he directory the program is stored in and combine it with
//the file name of the XML file.

var filepath = Path.Combine(AppContext.BaseDirectory, "WeatherAPI.xml");
//builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(options =>
{
    //adds some customised heading details to your swagger UI
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Weather API", Version = "v1" });
    //tells the swagger UI to read and import the XML comments from the XML file we
    //specified earlier
    options.IncludeXmlComments(filepath);


    options.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme
    {
        Description = "Enter your API key here to manage user access",
        Name = "apiKey",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                Type = ReferenceType.SecurityScheme,
                Id = "apiKey"

                },
                Name = "apiKey",
                In = ParameterLocation.Header
            },
            new List<string>()
        }

    });
});


//Add CORS to our API to manage which domains outside the API can talk to it.
builder.Services.AddCors(options =>
{
    //Create a policy that defines the rules for any CORS interactions
    options.AddPolicy("Google", p =>
    {
        p.WithOrigins("https://www.google.com", "https://www.google.com.au");
        p.AllowAnyHeader();
        p.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH");
    });
});

//Map our connection string setting into the settings class and add it to the dependency injection for
// fast retrieval when its needed
builder.Services.Configure<MongoConnectionSettings>(builder.Configuration.GetSection("ConnString"));

// add any required classes to the dependency injection system that needs to be shared by the system
builder.Services.AddScoped<MongoConnectionBuilder>();

builder.Services.AddScoped<IWeatherRepository, MongoWeatherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("Google");

app.MapControllers();

app.Run();
