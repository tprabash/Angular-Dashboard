using API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ensure the appsettings.json file is loaded
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Allow credentials if needed
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Retrieve connection string
var connectionString = builder.Configuration.GetConnectionString("EmployeeDatabase");

// Log the connection string for debugging purposes
Console.WriteLine($"Connection String: {connectionString}");

if (string.IsNullOrEmpty(connectionString))
{
    throw new ArgumentNullException("Connection string 'EmployeeDatabase' is null or empty.");
}

// Configure DbContexts
builder.Services.AddDbContext<DepartmentDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();

app.Run();
