using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//JWT
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("DefaultPolicy", policy =>
//        policy.RequireAuthenticatedUser());
//});

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


app.UseHttpsRedirection();

app.MapGet("/swagger/index.html", () => Results.Content(@" <!doctype html>
<html> 
<head><meta charset='utf-8'><title>API Docs Gateway</title></head>
<body style='font-family:Arial;padding:20px'> 
<h2>API Services (Swagger UI)</h2> 
<ul>
<li><a href='http://localhost:8080/swagger/index.html' target='_blank'>📘 TermAdmission Service</a></li>
<br/>
<li><a href='http://localhost:5022/swagger/index.html' target='_blank'>👤 User Service</a></li> 
<br/>
</ul> 
</body> 
</html>", "text/html"));

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");


var proxy = app.MapReverseProxy();
//proxy.RequireAuthorization("DefaultPolicy"); 

app.Run();
