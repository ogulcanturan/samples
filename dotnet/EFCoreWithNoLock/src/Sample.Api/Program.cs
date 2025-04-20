using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Sample.Api.DataContext;
using Sample.Api.Observers;

EfGlobalListener.Start(); // Adds a global listener for EF Core events. ( Includes SqlWithNoLockObserver )

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(cfg => cfg.AddSimpleConsole(opts =>
{
    opts.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ-";
    opts.ColorBehavior = LoggerColorBehavior.Enabled;
}));

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

await SeedData.EnsurePopulatedAsync(app); // Create database and ensure contains sample data.

app.MapGet("/products", async (SampleApiDbContext dbContext, HttpContext httpContext) =>
{
    var products = await dbContext.Products.AsNoTracking().ToArrayAsync(httpContext.RequestAborted);

    return Results.Ok(products);
});

app.Run();

EfGlobalListener.Stop(); // Stops the global listener for EF Core events.