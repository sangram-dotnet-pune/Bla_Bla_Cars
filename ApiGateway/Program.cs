using Ocelot.DependencyInjection;
using Ocelot.Middleware;

Environment.SetEnvironmentVariable(
    "DOTNET_hostBuilder:reloadConfigOnChange",
    "false"
);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.2

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var ocelotConfigFile = builder.Configuration["OCELOT_CONFIG_FILE"] ?? "ocelot.json";
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: false);
builder.Services.AddOcelot();

builder.Services.AddCors(options =>
{
    
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
  
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.UseWebSockets();
app.MapControllers();
await app.UseOcelot();

app.Run();
