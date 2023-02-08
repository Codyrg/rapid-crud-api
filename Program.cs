using Api;
using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TanvirArjel.EFCore.GenericRepository;

// Parse .env path from command line arguments with the format --env <PATH>
var envPath = args.Length > 1 && args[0] == "--env" ? args[1] : ".env";
Console.WriteLine($"Using .env file at {envPath}");
if(string.IsNullOrEmpty(envPath) || !File.Exists(envPath))
    throw new ArgumentException("Invalid .env path. Please provide a valid path to the .env file using the --env option.");
DotEnv.Load(new DotEnvOptions ( envFilePaths: new[] { envPath } ) );

var builder = WebApplication.CreateBuilder(args);

// Set logging level
var level = Environment.GetEnvironmentVariable("LOG_LEVEL")?.ToLower() switch 
{
    "information" => LogLevel.Information,
    "warning" => LogLevel.Warning,
    "error" => LogLevel.Error,
    "critical" => LogLevel.Critical,
    "debug" => LogLevel.Debug,
    "trace" => LogLevel.Trace,
    _ => LogLevel.None,
};
if(level == LogLevel.None)
    throw new ArgumentException("Invalid log level. Please provide a valid log level using the LOG_LEVEL environment variable. Valid values are Debug, Information, Warning, Error, Critical.");
builder.Logging.SetMinimumLevel(level);

// Set up database
var fileRoot = Environment.GetEnvironmentVariable("FILE_ROOT");
if(string.IsNullOrEmpty(fileRoot))
    throw new ArgumentException("Invalid file root. Please provide a valid file root using the FILE_ROOT environment variable.");
if(!Directory.Exists(fileRoot))
    Console.WriteLine($"Creating file root at {fileRoot}");
    Directory.CreateDirectory(fileRoot);
var databaseFilePath = $"{fileRoot}\\database.db";
var connectionString = $"Data Source={fileRoot}\\database.db";
builder.Services.AddDbContext<Database>(options => options.UseSqlite($"{connectionString}"));
builder.Services.AddGenericRepository<Database>();

// Set up authorization service
builder.Services.AddScoped<AuthorizerService>();

// create all tables and root user
using var scope = builder.Services.BuildServiceProvider().CreateScope();
using var context = scope.ServiceProvider.GetRequiredService<Database>();
context.Database.EnsureCreated();
var rootKey = Environment.GetEnvironmentVariable("ROOT_API_KEY");
Console.WriteLine($"Using root API key {rootKey}");
if(!Guid.TryParse(rootKey, out var rootId))
    throw new ArgumentException("Invalid root API key. Please provide a valid root API key using the ROOT_API_KEY environment variable.");
if(!context.ApiKeys.Any(x => x.Id == rootId))
{
    var rootApiKey = await context.ApiKeys.FirstOrDefaultAsync(x => x.IsRoot);
    if(rootApiKey != null)
        context.ApiKeys.Remove(rootApiKey);
    context.ApiKeys.Add(new ApiKey(rootId, "Root", true));
}
context.SaveChanges();

// Set up routes
var app = builder.Build();
app.MapGet("/", () => new { data = new { message = "Hello World!" } });

// Routes for managing API keys
app.MapAuthorizedPost("api/keys", async ([FromBody] string name, [FromServices] AuthorizerService authorizer) => await authorizer.CreateApiKey(name));
app.MapAuthorizedDelete("api/keys/{key}", async ([FromRoute] string key, [FromServices] AuthorizerService authorizer) => await authorizer.DeleteApiKey(key));

var port = Environment.GetEnvironmentVariable("PORT");
if(string.IsNullOrEmpty(port))
    port = "80";
Console.WriteLine($"Using port {port}");
app.Run($"http://localhost:{port}");