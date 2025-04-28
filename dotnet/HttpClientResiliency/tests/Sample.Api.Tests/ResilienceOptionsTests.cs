using Microsoft.Extensions.Http.Resilience;
using Sample.Api.Resilience;
using System.Threading.RateLimiting;

namespace Sample.Api.Tests
{
    public class ResilienceOptionsTests
    {
        [Fact]
        public void ShouldInitializeWithEmptyDictionaries()
        {
            // Act
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Assert
            Assert.NotNull(resilienceOptions.RateLimiterStrategies);
            Assert.Empty(resilienceOptions.RateLimiterStrategies);

            Assert.NotNull(resilienceOptions.TotalRequestTimeoutStrategies);
            Assert.Empty(resilienceOptions.TotalRequestTimeoutStrategies);

            Assert.NotNull(resilienceOptions.RetryStrategies);
            Assert.Empty(resilienceOptions.RetryStrategies);

            Assert.NotNull(resilienceOptions.CircuitBreakerStrategies);
            Assert.Empty(resilienceOptions.CircuitBreakerStrategies);

            Assert.NotNull(resilienceOptions.AttemptTimeoutStrategies);
            Assert.Empty(resilienceOptions.AttemptTimeoutStrategies);
        }
    }
}