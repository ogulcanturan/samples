using Sample.Api.Wcf;
using System.ServiceModel;
using Wcf.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWcfCaller<ISimpleWcfService>>(_ =>
{
    var binding = new BasicHttpBinding();
    var endpointAddress = new EndpointAddress("http://localhost:49334/SimpleWcfService.svc");

    return new WcfCaller<ISimpleWcfService>(binding, endpointAddress);
});

var app = builder.Build();

app.MapGet("/", (IWcfCaller<ISimpleWcfService> wcfCaller) =>
{
    var result = wcfCaller.Call(s => s.GetData(1));

    return Results.Ok(result);
});

app.Run();