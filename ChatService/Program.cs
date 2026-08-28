using ChatService.Hub;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddDbContext<ChatDbContext>(options =>
{
    options.UseNpgsql(GetDatabaseConnectionString(builder.Configuration));
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGatewayAndFrontend", policy =>
    {
        policy.WithOrigins(
                "https://localhost:5001", // gateway
                "http://localhost:5173",
                "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowGatewayAndFrontend");

app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapControllers();
app.Run();

static string GetDatabaseConnectionString(IConfiguration configuration) =>
    configuration["SUPABASE_CONNECTION_STRING"]
    ?? configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Set SUPABASE_CONNECTION_STRING to your Supabase PostgreSQL connection string.");
