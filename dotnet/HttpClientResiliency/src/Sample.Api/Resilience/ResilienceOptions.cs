using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Threading.RateLimiting;

namespace Sample.Api.Resilience
{
    public class ResilienceOptions : IOptions<ResilienceOptions>
    {
        public IReadOnlyDictionary<string, ConcurrencyLimiterOptions> RateLimiterStrategies { get; init; } = new ReadOnlyDictionary<string, ConcurrencyLimiterOptions>(new Dictionary<string, ConcurrencyLimiterOptions>(StringComparer.OrdinalIgnoreCase));

        public IReadOnlyDictionary<string, TimeSpan> TotalRequestTimeoutStrategies { get; init; } = new ReadOnlyDictionary<string, TimeSpan>(new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase));

        public IReadOnlyDictionary<string, HttpRetryStrategyOptions> RetryStrategies { get; init; } = new ReadOnlyDictionary<string, HttpRetryStrategyOptions>(new Dictionary<string, HttpRetryStrategyOptions>(StringComparer.OrdinalIgnoreCase));

        public IReadOnlyDictionary<string, HttpCircuitBreakerStrategyOptions> CircuitBreakerStrategies { get; init; } = new ReadOnlyDictionary<string, HttpCircuitBreakerStrategyOptions>(new Dictionary<string, HttpCircuitBreakerStrategyOptions>(StringComparer.OrdinalIgnoreCase));

        public IReadOnlyDictionary<string, TimeSpan> AttemptTimeoutStrategies { get; init; } = new ReadOnlyDictionary<string, TimeSpan>(new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase));

        ResilienceOptions IOptions<ResilienceOptions>.Value => this;
    }
}