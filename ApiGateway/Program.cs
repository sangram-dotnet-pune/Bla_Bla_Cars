using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.2

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOcelot(); // Register Ocelot services
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true); // Load Ocelot configuration

builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowReact",
    //     //policy =>
    //     //{
    //     //    policy
    //     //        .WithOrigins("http://localhost:5173") // React Vite URL
    //     //        .AllowAnyHeader()
    //     //        .AllowAnyMethod()
    //     //        .AllowCredentials();

    //     //});
         options.AddPolicy("AllowAll", policy =>
         {
             policy
                 .AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();
         });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");  // ? Enable CORS

app.UseAuthorization();

app.MapControllers();
await app.UseOcelot();

app.Run();
