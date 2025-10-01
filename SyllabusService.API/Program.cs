using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SyllabusService.API.Handler;
using SyllabusService.API.SchemaFilter;
using SyllabusService.Application.Services;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Domain;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Repositories;
using SyllabusService.Infrastructure.Repositories.IRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<PES_APP_FULL_DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<TimeOnlySchemaFilter>();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<TokenForwardingHandler>();

builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
{
    if (builder.Environment.IsDevelopment())
    {
        client.BaseAddress = new Uri("http://gateway.api:5000/auth-api/");
    }
    else
    {
        client.BaseAddress = new Uri("https://pesapp.orangeglacier-1e02abb7.southeastasia.azurecontainerapps.io/auth-api/");
    }

  
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<TokenForwardingHandler>()
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
// Retry 3 lần, mỗi lần chờ lâu dần theo lũy thừa 2 (2^retryAttempt giây)
//    // Lần 1: 2s, Lần 2: 4s, Lần 3: 8s
.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Add services to the container.

builder.Services.AddScoped<ISyllabusRepository, SyllabusRepository>();
builder.Services.AddScoped<IClassRepository, ClassesRepository>();

builder.Services.AddScoped<IClassesServices, ClassesService>(); 
builder.Services.AddScoped<ISyllabusService, SyllabusServ>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{

    var serverUrl = builder.Environment.IsDevelopment()
        ? "http://localhost:5000/syllabus-api" 
        : "https://pesapp.orangeglacier-1e02abb7.southeastasia.azurecontainerapps.io/terms-api"; 

    options.AddServer(new OpenApiServer
    {
        Url = serverUrl
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token theo format: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
