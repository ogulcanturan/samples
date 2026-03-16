using System.Collections.Concurrent;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Sample.Api.Wcf;

public sealed class WcfCaller<TContract> : IWcfCaller<TContract>, IDisposable where TContract : class
{
    private readonly Lazy<ChannelFactory<TContract>> _lazyChannelFactory;
    private bool _disposed;
    private readonly int _maxRetained;
    private int _retained;
    private readonly ConcurrentQueue<TContract> _queue = new();

    public WcfCaller(Binding binding, EndpointAddress address, int? maxRetained = null)
    {
        _maxRetained = maxRetained ?? GetDefaultMaxRetained(binding);

        if (_maxRetained <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetained));
        }

        _lazyChannelFactory = new Lazy<ChannelFactory<TContract>>(() =>
        {
            var factory = new ChannelFactory<TContract>(binding, address);
            factory.Open();
            return factory;
        }, isThreadSafe: true);
    }

    public TResult Call<TResult>(Func<TContract, TResult> call)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var client = GetLease();
        var abort = false;

        try
        {
            return call(client);
        }
        catch
        {
            abort = true;
            throw;
        }
        finally
        {
            Return(client, abort);
        }
    }

    public async Task<TResult> CallAsync<TResult>(Func<TContract, Task<TResult>> call)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var client = GetLease();
        var abort = false;

        try
        {
            return await call(client).ConfigureAwait(false);
        }
        catch
        {
            abort = true;
            throw;
        }
        finally
        {
            Return(client, abort);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_queue.TryDequeue(out var client))
        {
            if (client is ICommunicationObject comm)
            {
                SafelyCloseOrAbort(comm);
            }
        }

        if (!_lazyChannelFactory.IsValueCreated)
        {
            return;
        }

        var channelFactory = _lazyChannelFactory.Value;

        SafelyCloseOrAbort(channelFactory);
    }

    private static bool TryOpen(ICommunicationObject comm)
    {
        try
        {
            comm.Open();
            return true;
        }
        catch
        {
            SafelyAbort(comm);
            return false;
        }
    }

    private static void SafelyAbort(ChannelFactory<TContract> channelFactory)
    {
        try
        {
            channelFactory.Abort();
        }
        catch
        {
        }
    }

    private static void SafelyAbort(ICommunicationObject comm)
    {
        try
        {
            comm.Abort();
        }
        catch
        {
        }
    }
    private static void SafelyCloseOrAbort(ICommunicationObject comm)
    {
        if (comm.State == CommunicationState.Faulted)
        {
            SafelyAbort(comm);
            return;
        }

        try
        {
            comm.Close();
        }
        catch
        {
            SafelyAbort(comm);
        }
    }

    private static void SafelyCloseOrAbort(ChannelFactory<TContract> channelFactory)
    {
        if (channelFactory.State == CommunicationState.Faulted)
        {
            SafelyAbort(channelFactory);
            return;
        }

        try
        {
            channelFactory.Close();
        }
        catch
        {
            SafelyAbort(channelFactory);
        }
    }

    private TContract GetLease()
    {
        if (!_queue.TryDequeue(out var client))
        {
            return Create();
        }

        Interlocked.Decrement(ref _retained);

        if (client is not ICommunicationObject comm)
        {
            return client;
        }

        if (comm.State == CommunicationState.Faulted)
        {
            SafelyAbort(comm);
            return Create();
        }

        if (comm.State != CommunicationState.Opened && !TryOpen(comm))
        {
            return Create();
        }

        return client;
    }

    private void Return(TContract client, bool abort)
    {
        if (client is not ICommunicationObject comm)
        {
            return;
        }

        if (abort || comm.State == CommunicationState.Faulted)
        {
            SafelyAbort(comm);
            return;
        }

        var isPoolFull = Interlocked.Increment(ref _retained) > _maxRetained;

        if (isPoolFull)
        {
            Interlocked.Decrement(ref _retained);
            SafelyCloseOrAbort(comm);
            return;
        }

        _queue.Enqueue(client);
    }

    private TContract Create()
    {
        var channel = _lazyChannelFactory.Value.CreateChannel();
        ((ICommunicationObject)channel).Open();
        return channel;
    }

    private static int GetDefaultMaxRetained(Binding binding)
    {
        var serviceContractAttr = typeof(TContract).GetCustomAttribute<ServiceContractAttribute>(inherit: true);

        var sessionMode = serviceContractAttr?.SessionMode ?? SessionMode.Allowed;

        var sessionEnabled =
            sessionMode == SessionMode.Required ||
            binding is NetTcpBinding ||
            (binding is WSHttpBinding ws && ws.ReliableSession.Enabled) ;

        return sessionEnabled ? 12 : 32;
    }
}