using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Sample.Api;
using Sample.Api.DataContext;
using Sample.Api.Observers;

// Adds a global listener for EF Core events. ( Includes SqlWithNoLockObserver )
EfGlobalListener.Start(); 

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(opts =>
{
    opts.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ-";
    opts.ColorBehavior = LoggerColorBehavior.Enabled;
});

// Retrieving connection string from the appsettings.json
var connectionStringForSqlServer = builder.Configuration.GetConnectionString("SqlServer"); 

builder.Services.AddDbContext<SampleApiDbContext>(opts =>
{
    opts.UseSqlServer(connectionStringForSqlServer);

    if (builder.Environment.IsDevelopment())
    {
        opts.EnableSensitiveDataLogging();
    }
});

var app = builder.Build();

// Create database and ensure contains sample data.
await SeedData.EnsurePopulatedAsync(app); 

app.MapGet("/products", async (SampleApiDbContext dbContext, HttpContext httpContext) =>
{
    var products = await dbContext.Products.WithNoLock().ToArrayAsync(httpContext.RequestAborted);

    return Results.Ok(products);
});

app.Run();

// Stops the global listener for EF Core events.
EfGlobalListener.Stop(); 