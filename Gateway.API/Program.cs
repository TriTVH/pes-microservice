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
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; }) 
    .AddJwtBearer(options => { options.RequireHttpsMetadata = false;
    options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"], IssuerSigningKey = new SymmetricSecurityKey(keyBytes) };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("id")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.HttpContext.Items["UserId"] = userId;
                }

                return Task.CompletedTask;
            }
        };
    });



builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
     .AddTransforms(builderContext =>
     {
         builderContext.AddResponseTransform(async transformContext =>
         {
             // Chỉ forward khi có body
             if (transformContext.HttpContext.Response.ContentLength == 0 &&
                 transformContext.ProxyResponse?.Content != null)
             {
                 await transformContext.ProxyResponse.Content.CopyToAsync(transformContext.HttpContext.Response.Body);
             }
         });
     }); ;

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ParentAndEducationAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("PARENT", "EDUCATION"); // Yêu cầu role PARENT hoặc EDUCATION
    });
    options.AddPolicy("EducationAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("EDUCATION"); // Yêu cầu role ED C U A T I O N
    });
    options.AddPolicy("ParentAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("PARENT");
    });
});

builder.Services.AddCors(options => { options.AddPolicy("AllowAll",    
    policy => { policy.AllowAnyOrigin() .AllowAnyMethod() .AllowAnyHeader(); }); });
var app = builder.Build(); 
app.UseHttpsRedirection();
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        // Nếu request từ client có Authorization header thì forward xuống
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var token = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers["Authorization"] = token;
            }
        }


        if (context.User?.Identity?.IsAuthenticated == true)
        {
            if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userId)
            {
                // Forward Id sang downstream service
                context.Request.Headers["X-User-Id"] = userId;
            }
        }

        await next();
    });
}); 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // NOTE: these URLs are the gateway proxy endpoints we defined above
    c.SwaggerEndpoint("/swagger/proxy/spec/auth", "Auth API");
    c.SwaggerEndpoint("/swagger/proxy/spec/class", "Class API");
    c.SwaggerEndpoint("/swagger/proxy/spec/parent", "Parent API");

});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization(); 
app.Run();