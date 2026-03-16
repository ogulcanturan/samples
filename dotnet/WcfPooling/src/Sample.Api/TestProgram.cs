//using Microsoft.Extensions.Logging.Console;
//using Sample.Api.Wcf;
//using System.Diagnostics;
//using System.ServiceModel;
//using Wcf.Abstractions;

//var builder = WebApplication.CreateBuilder(args);

//builder.Logging.AddSimpleConsole(cfg =>
//{
//    cfg.TimestampFormat = "[dd-MM-yyyyTHH:mm:ss.fffffffK]-";
//    cfg.ColorBehavior = LoggerColorBehavior.Enabled;
//});

//var binding = new BasicHttpBinding();

//var endpointAddress = new EndpointAddress("http://localhost:49334/SimpleWcfService.svc");

//var wcfCaller = new WcfCaller<ISimpleWcfService>(binding, endpointAddress);

//builder.Services.AddSingleton<IWcfCaller<ISimpleWcfService>>(_ =>
//{
//    return new WcfCaller<ISimpleWcfService>(binding, endpointAddress);
//});

//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.MapGet("/wcf/with-pooling", async () =>
//{
//    var elapsedMilliSeconds = await RunBatchAsync(() => Task.Run(() => wcfCaller.Call(s => s.GetData(1))));

//    return Results.Ok($"Elapsed milliseconds: {elapsedMilliSeconds}");
//});


//var channelFactory = new ChannelFactory<ISimpleWcfService>(binding, endpointAddress);

//app.MapGet("/wcf/without-pooling", async () =>
//{
//    var elapsedMilliSeconds = await RunBatchAsync(() => Task.Run(() =>
//    {
//        var channel = channelFactory.CreateChannel();

//        try
//        {
//            var result= wcfCaller.Call(s => s.GetData(1));
//            ((ICommunicationObject)channel).Close();
//        }
//        catch
//        {
//            try
//            {
//                ((ICommunicationObject)channel).Abort();
//            }
//            catch { }

//            throw;
//        }

//        return wcfCaller.Call(s => s.GetData(1));
//    }));

//    return Results.Ok($"Elapsed milliseconds: {elapsedMilliSeconds}");
//});

//app.Run();

//return;

//async Task<long> RunBatchAsync(Func<Task> func)
//{
//    var tasks = Enumerable.Range(0, 1000).Select(_ => func());

//    var stopwatch = Stopwatch.StartNew();

//    await Task.WhenAll(tasks);

//    stopwatch.Stop();

//    return stopwatch.ElapsedMilliseconds;
//}

//void DisposeChannelFactory(ChannelFactory<ISimpleWcfService> factory)
//{
//    if (factory.State == CommunicationState.Faulted)
//    {
//        try
//        {
//            factory.Abort();
//        }
//        catch
//        {
//        }
//    }
//    else
//    {
//        try
//        {
//            factory.Close();
//        }
//        catch
//        {
//            try
//            {
//                factory.Abort();
//            }
//            catch
//            {
//            }
//        }
//    }
    
//}