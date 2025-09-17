using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TermAdmissionManagement.Application.Services;
using TermAdmissionManagement.Application.Services.IService;
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
builder.Services.AddScoped<ITermItemRepository, TermItemRepository>();
builder.Services.AddScoped<IAdmissionTermService, AdmissionTermService>();

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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PesTermManagementContext>();
    context.Database.EnsureCreated(); // hoặc context.Database.Migrate() nếu có migrations
}

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
