using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);




// Load appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

// Get MongoDB settings
var mongoSettings = builder.Configuration.GetSection("MongoDB");
string connectionString = mongoSettings.GetValue<string>("ConnectionString");
string databaseName = mongoSettings.GetValue<string>("DatabaseName");


Console.WriteLine($"MongoDB Connection String: {connectionString ?? "NULL"}");
Console.WriteLine($"Database Name: {databaseName ?? "NULL"}");

// Register MongoDB Client
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

// Register MongoDbService
builder.Services.AddSingleton<MongoDbService>();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers();


var app = builder.Build();

// Use PORT from environment (important for Render or any cloud)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.UseCors("AllowAllOrigins");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
