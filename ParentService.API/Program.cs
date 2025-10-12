using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParentService.Application.Services;
using ParentService.Application.Services.IServices;
using ParentService.Domain;
using ParentService.Domain.IClient;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories;
using ParentService.Infrastructure.Repositories.IRepositories;
using Polly;
using Polly.Extensions.Http;
using SyllabusService.API.Handler;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddDbContext<PES_APP_FULL_DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IAdmissionFormRepo, AdmissionFormRepo>();
builder.Services.AddScoped<IStudentRepo, StudentRepo>();

builder.Services.AddScoped<IAdmissionFormService, AdmissionFormService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<TokenForwardingHandler>();

builder.Services.AddHttpClient<IClassServiceClient, ClassServiceClient>(client =>
{
    if (builder.Environment.IsDevelopment())
    {
        client.BaseAddress = new Uri("http://gateway.api:5000/class-api/");
    }
    else
    {
        client.BaseAddress = new Uri("https://pesapp.orangeglacier-1e02abb7.southeastasia.azurecontainerapps.io/class-api/");
    }


    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<TokenForwardingHandler>()
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
// Retry 3 lần, mỗi lần chờ lâu dần theo lũy thừa 2 (2^retryAttempt giây)
//    // Lần 1: 2s, Lần 2: 4s, Lần 3: 8s
.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Parent Service API",
        Version = "v1"
    });
    var serverUrl = builder.Environment.IsDevelopment()
        ? "http://localhost:5000/parent-api"
        : "https://pesapp.orangeglacier-1e02abb7.southeastasia.azurecontainerapps.io/parent-api";

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

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.Run();
