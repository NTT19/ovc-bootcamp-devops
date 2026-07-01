using Microsoft.AspNetCore.HttpOverrides;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);


// =======================
// JWT Authentication
// =======================

builder.Services.AddAuthentication()
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,

        ValidateIssuer = false,
        ValidateAudience = false,

        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration
                    .GetSection("AppSettings:Token")
                    .Value!
                ))
    };
});



// =======================
// Controllers
// =======================

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        ReferenceHandler.IgnoreCycles;
});



// =======================
// Database
// =======================

builder.Services.AddDbContext<FinalContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration
        .GetConnectionString("dbContext")
    );
});



// =======================
// CORS
// =======================

builder.Services.AddCors(options =>
{

    var allowedOrigins =
        builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>();


    options.AddPolicy("CorsPolicy", policy =>
    {

        policy
            .WithOrigins(
                allowedOrigins ?? Array.Empty<string>()
            )
            .AllowAnyHeader()
            .AllowAnyMethod();

    });

});



// =======================
// Swagger
// =======================

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{

    options.AddSecurityDefinition(
        "oauth2",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });


    options.OperationFilter<SecurityRequirementsOperationFilter>();

});




// =======================
// Build App
// =======================

var app = builder.Build();



// =======================
// Reverse Proxy
// Nginx / Docker
// =======================

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});




// =======================
// Swagger
// Enable all environments
// =======================

app.UseSwagger();

app.UseSwaggerUI();



// =======================
// CORS
// =======================

app.UseCors("CorsPolicy");



// =======================
// Authentication
// Authorization
// =======================

app.UseAuthentication();

app.UseAuthorization();



// =======================
// Controllers
// =======================

app.MapControllers();



// =======================
// Run
// =======================

app.Run();