
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using TermAdmissionManagement.API.Handler;
using TermAdmissionManagement.Application.Services;
using TermAdmissionManagement.Application.Services.IService;
using TermAdmissionManagement.Domain;
using TermAdmissionManagement.Domain.IClients;
using TermAdmissionManagement.Infrastructure.DBContext;
using TermAdmissionManagement.Infrastructure.Repositories;
using TermAdmissionManagement.Infrastructure.Repositories.IRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<PesTermManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAdmissionTermRepository, AdmissionTermRepository>();
builder.Services.AddScoped<IAdmissionFormRepository, AdmissionFormRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<TokenForwardingHandler>();

//builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
//{
//  // Đặt BaseAddress mặc định cho HttpClient này
//    client.BaseAddress = new Uri("http://gateway.api:5000/auth-api/");
// // Khi gọi, bạn chỉ cần viết client.GetAsync("api/term/123") thay vì full URL
// // Đặt timeout cho mỗi request là 30 giây
//    client.Timeout = TimeSpan.FromSeconds(30);
//}).AddHttpMessageHandler<TokenForwardingHandler>()
//    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
//// Retry 3 lần, mỗi lần chờ lâu dần theo lũy thừa 2 (2^retryAttempt giây)
////    // Lần 1: 2s, Lần 2: 4s, Lần 3: 8s
//.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));



builder.Services.AddScoped<IAdmissionFormService, AdmissionFormService>();
builder.Services.AddScoped<IAdmissionTermService, AdmissionTermService>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(new OpenApiServer
    {
        Url = "https://pesapp.orangeglacier-1e02abb7.southeastasia.azurecontainerapps.io"  // Gateway URL
    });
    // Thêm security definition cho JWT
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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PesTermManagementContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
