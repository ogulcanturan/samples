using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Sample.Api.Resilience
{
    internal class ResilienceConfigurationValidator<TResilienceConfiguration>(
       ResilienceOptions resilienceOptions) : IValidateOptions<TResilienceConfiguration>
       where TResilienceConfiguration : class, IResilienceConfiguration
    {
        public ValidateOptionsResult Validate(string name, TResilienceConfiguration options)
        {
            var builder = new ValidateOptionsResultBuilder();

            var hasRateLimiter = options.ResilienceStrategies.TryGetValue(ResilienceStrategy.RateLimiter, out var rateLimiterStrategyKey);

            if (hasRateLimiter)
            {
                ValidateRateLimiterStrategy(rateLimiterStrategyKey, builder);
            }

            var hasTotalRequestTimeoutStrategy = options.ResilienceStrategies.TryGetValue(ResilienceStrategy.TotalRequestTimeout, out var totalRequestTimeoutStrategyKey);

            if (hasTotalRequestTimeoutStrategy)
            {
                ValidateTotalRequestTimeoutStrategy(totalRequestTimeoutStrategyKey, builder);
            }

            var hasRetryStrategy = options.ResilienceStrategies.TryGetValue(ResilienceStrategy.Retry, out var retryStrategyKey);

            if (hasRetryStrategy)
            {
                ValidateRetryStrategy(retryStrategyKey, builder);
            }

            var hasCircuitBreakerStrategy = options.ResilienceStrategies.TryGetValue(ResilienceStrategy.CircuitBreaker, out var circuitBreakerStrategyKey);

            HttpCircuitBreakerStrategyOptions circuitBreaker = null;

            if (hasCircuitBreakerStrategy)
            {
                ValidateCircuitBreakerStrategy(circuitBreakerStrategyKey, builder, out circuitBreaker);
            }

            var hasAttemptTimeoutStrategy = options.ResilienceStrategies.TryGetValue(ResilienceStrategy.AttemptTimeout, out var attemptTimeoutStrategyKey);

            if (!hasAttemptTimeoutStrategy ||
                !IsAttemptTimeoutStrategyValid(attemptTimeoutStrategyKey, builder, out var attemptTimeout) ||
                !attemptTimeout.HasValue)
            {
                return builder.Build();
            }

            if (!string.IsNullOrWhiteSpace(totalRequestTimeoutStrategyKey))
            {
                var totalRequestTimeout = resilienceOptions.TotalRequestTimeoutStrategies[totalRequestTimeoutStrategyKey];

                if (attemptTimeout > totalRequestTimeout)
                {
                    builder.AddError($"Total request timeout resilience strategy must have a greater timeout than the attempt resilience strategy. Total Request Timeout: {totalRequestTimeout.TotalSeconds}s, Attempt Timeout: {attemptTimeout.Value.TotalSeconds}s.");
                }
            }

            if (!hasCircuitBreakerStrategy)
            {
                return builder.Build();
            }

            if (circuitBreaker?.SamplingDuration < TimeSpan.FromMilliseconds(attemptTimeout.Value.TotalMilliseconds * 2.0))
            {
                builder.AddError($"The sampling duration of circuit breaker strategy needs to be at least double of an attempt timeout strategy’s timeout interval, in order to be effective. Sampling Duration: {circuitBreaker.SamplingDuration.TotalSeconds}s, Attempt Timeout: {attemptTimeout.Value.TotalSeconds}s.");
            }

            return builder.Build();
        }

        private void ValidateRateLimiterStrategy(string rateLimiterStrategyKey, ValidateOptionsResultBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(rateLimiterStrategyKey))
            {
                AddKeyEmptyError(builder, nameof(ResilienceStrategy.RateLimiter));

                return;
            }

            if (resilienceOptions.RateLimiterStrategies.ContainsKey(rateLimiterStrategyKey))
            {
                return;
            }

            AddKeyNotFoundError(builder, nameof(ResilienceStrategy.RateLimiter), rateLimiterStrategyKey);
        }

        private void ValidateTotalRequestTimeoutStrategy(string totalRequestTimeoutStrategyKey, ValidateOptionsResultBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(totalRequestTimeoutStrategyKey))
            {
                AddKeyEmptyError(builder, nameof(ResilienceStrategy.TotalRequestTimeout));

                return;
            }

            if (resilienceOptions.TotalRequestTimeoutStrategies.ContainsKey(totalRequestTimeoutStrategyKey))
            {
                return;
            }

            AddKeyNotFoundError(builder, nameof(ResilienceStrategy.TotalRequestTimeout), totalRequestTimeoutStrategyKey);
        }

        private void ValidateRetryStrategy(string retryStrategyKey, ValidateOptionsResultBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(retryStrategyKey))
            {
                AddKeyEmptyError(builder, nameof(ResilienceStrategy.Retry));

                return;
            }

            if (resilienceOptions.RetryStrategies.ContainsKey(retryStrategyKey))
            {
                return;
            }

            AddKeyNotFoundError(builder, nameof(ResilienceStrategy.Retry), retryStrategyKey);
        }

        private void ValidateCircuitBreakerStrategy(string circuitBreakerStrategyKey, ValidateOptionsResultBuilder builder, out HttpCircuitBreakerStrategyOptions? circuitBreaker)
        {
            if (string.IsNullOrWhiteSpace(circuitBreakerStrategyKey))
            {
                AddKeyEmptyError(builder, nameof(ResilienceStrategy.CircuitBreaker));

                circuitBreaker = null;

                return;
            }

            if (resilienceOptions.CircuitBreakerStrategies.TryGetValue(circuitBreakerStrategyKey, out var value))
            {
                circuitBreaker = value;

                return;
            }

            AddKeyNotFoundError(builder, nameof(ResilienceStrategy.CircuitBreaker), circuitBreakerStrategyKey);

            circuitBreaker = null;
        }

        private bool IsAttemptTimeoutStrategyValid(string attemptTimeoutStrategyKey, ValidateOptionsResultBuilder builder, out TimeSpan? attemptTimeout)
        {
            if (string.IsNullOrWhiteSpace(attemptTimeoutStrategyKey))
            {
                AddKeyEmptyError(builder, nameof(ResilienceStrategy.AttemptTimeout));

                attemptTimeout = null;

                return false;
            }

            if (resilienceOptions.AttemptTimeoutStrategies.TryGetValue(attemptTimeoutStrategyKey, out var value))
            {
                attemptTimeout = value;

                return true;
            }

            AddKeyNotFoundError(builder, nameof(ResilienceStrategy.AttemptTimeout), attemptTimeoutStrategyKey);

            attemptTimeout = null;

            return false;
        }

        public static void AddKeyEmptyError(ValidateOptionsResultBuilder builder, string propertyName)
        {
            builder.AddError($"The '{propertyName}' strategy key cannot be empty.", propertyName);
        }

        private static void AddKeyNotFoundError(ValidateOptionsResultBuilder builder, string propertyName, string strategyKey)
        {
            builder.AddError($"The strategy key: '{strategyKey}' for {propertyName} could not be obtained from the {nameof(ResilienceOptions)}. Check 'appsettings.json' to ensure {nameof(ResilienceOptions)} is correctly configured, and verify that {nameof(ServiceCollectionExtensions.AddResilienceOptions)}(...) is properly configured in Dependency Injection.", propertyName);
        }
    }
}