using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args); 

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

var jwt = builder.Configuration.GetSection("Jwt"); 
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!); 

builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; }) .AddJwtBearer(options => { options.RequireHttpsMetadata = false; options.SaveToken = true; options.TokenValidationParameters = new TokenValidationParameters 
  { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"], IssuerSigningKey = new SymmetricSecurityKey(keyBytes) };
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Forward claims qua header
                var userId = user.FindFirst("sub")?.Value ?? "";
                var roles = string.Join(",", user.FindAll("role").Select(r => r.Value));

                transformContext.ProxyRequest.Headers.Remove("X-User-Id");
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);

                transformContext.ProxyRequest.Headers.Remove("X-User-Roles");
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", roles);
            }
        });
    });


builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization();

builder.Services.AddCors(options => { options.AddPolicy("AllowAll",    
    policy => { policy.AllowAnyOrigin() .AllowAnyMethod() .AllowAnyHeader(); }); });
var app = builder.Build(); 
app.UseHttpsRedirection();
app.MapReverseProxy();
app.UseSwagger();
app.UseRouting();
app.UseSwaggerUI(c =>
{
    // NOTE: these URLs are the gateway proxy endpoints we defined above
    c.SwaggerEndpoint("/swagger/proxy/spec/terms", "Term API");
    c.SwaggerEndpoint("/swagger/proxy/spec/auth", "Auth API");
});
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization(); 
app.Run();