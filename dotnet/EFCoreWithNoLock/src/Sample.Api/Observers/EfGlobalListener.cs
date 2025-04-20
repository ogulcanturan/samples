using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Sample.Api.Observers
{
    public sealed class EfGlobalListener : IObserver<DiagnosticListener>, IDisposable
    {
        private static readonly Lazy<EfGlobalListener> Instance = new Lazy<EfGlobalListener>(() => new EfGlobalListener());

        private IDisposable _withNoLockSubscription;
        private IDisposable _efGlobalSubscription;

        private readonly SqlWithNoLockObserver _sqlWithNoLockObserver = new SqlWithNoLockObserver();

        private EfGlobalListener()
        {
        }

        public void Subscribe()
        {
            if(_efGlobalSubscription != null)
            {
                return;
            }

            _efGlobalSubscription = DiagnosticListener.AllListeners.Subscribe(this);
        }

        public void Dispose()
        {
            if (_efGlobalSubscription == null)
            {
                return;
            }

            _withNoLockSubscription?.Dispose();
            _efGlobalSubscription.Dispose();

            _withNoLockSubscription = null;
            _efGlobalSubscription = null;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DbLoggerCategory.Name)
            {
                _withNoLockSubscription = value.Subscribe(_sqlWithNoLockObserver);
            }
        }

        public static void Start()
        {
            Instance.Value.Subscribe();
        }

        public static void Stop()
        {
            Instance.Value.Dispose();
        }
    }
}