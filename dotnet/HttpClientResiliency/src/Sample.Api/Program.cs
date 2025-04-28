using Microsoft.Extensions.Logging.Console;
using Sample.Api;
using Sample.Api.Resilience;
using Sample.Api.SampleApiClient;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(opts =>
{
    opts.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ-";
    opts.ColorBehavior = LoggerColorBehavior.Enabled;
});

// Adding and validating ResilienceOptions from 'appsettings.json'.
builder.Services.AddResilienceOptions(builder.Configuration);

// Adding and validating SampleApiHttpClient from 'appsettings.json'.
builder.Services.AddSampleApiClient(builder.Configuration);

var app = builder.Build();

app.MapGet("/get-products-via-http-client", async (ISampleApiClient sampleApiHttpClient, HttpContext httpContext) =>
{
    var results = await sampleApiHttpClient.GetProductsAsync(httpContext.RequestAborted);

    return Results.Ok(results);
});

app.MapGet("/products", async () =>
{
    // Simulating a long-running task - Tweak number to observe, TotalRequestTimeout & AttemptTimeout
    // await Task.Delay(TimeSpan.FromSeconds(45));

    return Results.Ok(new List<object>
    {
        new { Id = 1, Name = "IPHONE 17 White 256gb", Quantity = 20  },
        new { Id = 2, Name = "IPHONE 17 Gold 512gb", Quantity = 10  }
    });
});

app.Run();