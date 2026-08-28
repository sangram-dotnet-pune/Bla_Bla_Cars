using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TripService.Models;
using TripService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
            Reference = new Microsoft.OpenApi.Models.OpenApiReference{
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }, new string[]{}
    }});
});
builder.Services.AddHttpClient<UserClientService>(client =>
{
    client.BaseAddress = GetRequiredServiceUrl(builder.Configuration, "GatewayService");
});
builder.Services.AddDbContext<TripDbContext>(options =>
{
    options.UseNpgsql(GetDatabaseConnectionString(builder.Configuration));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

static string GetDatabaseConnectionString(IConfiguration configuration) =>
    configuration["SUPABASE_CONNECTION_STRING"]
    ?? configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Set SUPABASE_CONNECTION_STRING to your Supabase PostgreSQL connection string.");

static Uri GetRequiredServiceUrl(IConfiguration configuration, string serviceName)
{
    var value = configuration[$"ServiceUrls:{serviceName}"];
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            $"ServiceUrls:{serviceName} is not configured.");
    }

    return new Uri(value, UriKind.Absolute);
}
