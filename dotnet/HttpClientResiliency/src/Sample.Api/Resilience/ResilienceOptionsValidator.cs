using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace Sample.Api.Resilience
{
    internal class ResilienceOptionsValidator : IValidateOptions<ResilienceOptions>
    {
        public static ValidateOptionsResult Validate(ResilienceOptions options)
        {
            return new ResilienceOptionsValidator().Validate(null, options);
        }

        public ValidateOptionsResult Validate(string name, ResilienceOptions options)
        {
            var builder = new ValidateOptionsResultBuilder();

            ValidateRateLimiterStrategies(options.RateLimiterStrategies, builder);
            ValidateTotalRequestTimeoutStrategies(options.TotalRequestTimeoutStrategies, builder);
            ValidateRetryStrategies(options.RetryStrategies, builder);
            ValidateCircuitBreakerStrategies(options.CircuitBreakerStrategies, builder);
            ValidateAttemptTimeoutStrategies(options.AttemptTimeoutStrategies, builder);

            return builder.Build();
        }

        private static void ValidateRateLimiterStrategies(IReadOnlyDictionary<string, ConcurrencyLimiterOptions> rateLimitStrategies, ValidateOptionsResultBuilder builder)
        {
            foreach (var (name, rateLimit) in rateLimitStrategies)
            {
                if (rateLimit.PermitLimit <= 0)
                {
                    builder.AddError($"{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.PermitLimit)} must be greater than 0.", $"""{nameof(ResilienceOptions.RateLimiterStrategies)}["{name}"].{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.PermitLimit)}""");
                }

                if (!Enum.IsDefined(rateLimit.QueueProcessingOrder))
                {
                    builder.AddError($"{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.QueueProcessingOrder)} must be 0 (OldestFirst) Or 1 (NewestFirst).", $"""{nameof(ResilienceOptions.RateLimiterStrategies)}["{name}"].{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.QueueProcessingOrder)}""");
                }

                if (rateLimit.QueueLimit < 0)
                {
                    builder.AddError($"{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.QueueLimit)} must be greater or equal to 0.", $"""{nameof(ResilienceOptions.RateLimiterStrategies)}["{name}"].{nameof(HttpRateLimiterStrategyOptions.DefaultRateLimiterOptions.QueueLimit)}""");
                }
            }
        }

        private static void ValidateTotalRequestTimeoutStrategies(IReadOnlyDictionary<string, TimeSpan> timeoutStrategies, ValidateOptionsResultBuilder builder)
        {
            var max = TimeSpan.FromDays(1);
            var min = TimeSpan.FromMilliseconds(10);

            foreach (var (name, timeout) in timeoutStrategies)
            {
                if (timeout > max)
                {
                    builder.AddError($"{nameof(ResilienceStrategy.TotalRequestTimeout)} must be less than a day.", $"""{nameof(ResilienceOptions.TotalRequestTimeoutStrategies)}["{name}"]""");
                }
                else if (min > timeout)
                {
                    builder.AddError($"{nameof(ResilienceStrategy.TotalRequestTimeout)} must be greater than 10ms.", $"""{nameof(ResilienceOptions.TotalRequestTimeoutStrategies)}["{name}"]""");
                }
            }
        }

        private static void ValidateRetryStrategies(IReadOnlyDictionary<string, HttpRetryStrategyOptions> retryStrategies, ValidateOptionsResultBuilder builder)
        {
            var max = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1);
            var min = TimeSpan.Zero;

            foreach (var (name, retry) in retryStrategies)
            {
                if (!Enum.IsDefined(retry.BackoffType))
                {
                    builder.AddError($"The {nameof(HttpRetryStrategyOptions.BackoffType)} must be 0 (Constant) Or 1 (Linear) Or Exponential (2).", $"""{nameof(ResilienceOptions.RetryStrategies)}["{name}"].{nameof(HttpRetryStrategyOptions.BackoffType)}""");
                }

                if (retry.Delay >= max)
                {
                    builder.AddError($"The {nameof(HttpRetryStrategyOptions.Delay)} must be less or equal a day.", $"""{nameof(ResilienceOptions.RetryStrategies)}["{name}"].{nameof(HttpRetryStrategyOptions.Delay)}""");
                }
                else if (min > retry.Delay)
                {
                    builder.AddError($"The {nameof(HttpRetryStrategyOptions.Delay)} must be greater than TimeSpan.Zero (0).", $"""{nameof(ResilienceOptions.RetryStrategies)}["{name}"].{nameof(HttpRetryStrategyOptions.Delay)}""");
                }

                if (!retry.MaxDelay.HasValue)
                {
                    continue;
                }

                if (retry.MaxDelay >= max)
                {
                    builder.AddError($"The {nameof(HttpRetryStrategyOptions.MaxDelay)} must be less than or equal a day, or it can be null.", $"""{nameof(ResilienceOptions.RetryStrategies)}["{name}"].{nameof(HttpRetryStrategyOptions.MaxDelay)}""");
                }
                else if (min > retry.MaxDelay)
                {
                    builder.AddError($"The {nameof(HttpRetryStrategyOptions.MaxDelay)} must be greater than or equal to TimeSpan.Zero (0), or it can be null.", $"""{nameof(ResilienceOptions.RetryStrategies)}["{name}"].{nameof(HttpRetryStrategyOptions.MaxDelay)}""");
                }
            }
        }

        private static void ValidateCircuitBreakerStrategies(IReadOnlyDictionary<string, HttpCircuitBreakerStrategyOptions> circuitBreakerStrategies, ValidateOptionsResultBuilder builder)
        {
            var max = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1);
            var min = TimeSpan.FromMilliseconds(500);

            foreach (var (name, circuitBreaker) in circuitBreakerStrategies)
            {
                if (circuitBreaker.FailureRatio > 1.0d)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.FailureRatio)} must be less or equal to 1.0.", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.FailureRatio)}""");
                }
                else if (0 > circuitBreaker.FailureRatio)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.FailureRatio)} must be greater or equal to 0.", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.FailureRatio)}""");
                }

                if (2 > circuitBreaker.MinimumThroughput)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.MinimumThroughput)} must be greater or equal to 2.", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.MinimumThroughput)}""");
                }

                if (circuitBreaker.SamplingDuration >= max)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.SamplingDuration)} must be less or equal a day.", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.SamplingDuration)}""");
                }
                else if (min > circuitBreaker.SamplingDuration)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.SamplingDuration)} must be greater than or equal to 500ms", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.SamplingDuration)}""");
                }

                if (circuitBreaker.BreakDuration >= max)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.BreakDuration)} must be less than or equal a day.", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.BreakDuration)}""");
                }
                else if (min > circuitBreaker.BreakDuration)
                {
                    builder.AddError($"The {nameof(HttpCircuitBreakerStrategyOptions.BreakDuration)} must be greater than or equal to 500ms", $"""{nameof(ResilienceOptions.CircuitBreakerStrategies)}["{name}"].{nameof(HttpCircuitBreakerStrategyOptions.BreakDuration)}""");
                }
            }
        }

        private static void ValidateAttemptTimeoutStrategies(IReadOnlyDictionary<string, TimeSpan> timeoutStrategies, ValidateOptionsResultBuilder builder)
        {
            var max = TimeSpan.FromDays(1);
            var min = TimeSpan.FromMilliseconds(10);

            foreach (var (name, timeout) in timeoutStrategies)
            {
                if (timeout > max)
                {
                    builder.AddError($"{nameof(ResilienceStrategy.AttemptTimeout)} must be less than a day.", $"""{nameof(ResilienceOptions.AttemptTimeoutStrategies)}["{name}"]""");
                }
                else if (min > timeout)
                {
                    builder.AddError($"{nameof(ResilienceStrategy.AttemptTimeout)} must be greater than 10ms.", $"""{nameof(ResilienceOptions.AttemptTimeoutStrategies)}["{name}"]""");
                }
            }
        }
    }
}