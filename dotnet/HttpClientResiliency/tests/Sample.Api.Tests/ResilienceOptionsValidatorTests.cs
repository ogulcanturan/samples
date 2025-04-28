using Microsoft.Extensions.Http.Resilience;
using Polly;
using Sample.Api.Resilience;
using System.Threading.RateLimiting;

namespace Sample.Api.Tests
{
    public class ResilienceOptionsValidatorTests
    {
        [Fact]
        public void Validate_ShouldReturnError_WhenRateLimiterStrategiesAreInvalid()
        {
            // Arrange
            var rateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>
            {
                {
                    "Standard-RateLimiter", new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 0,
                        QueueProcessingOrder = (QueueProcessingOrder)5,
                        QueueLimit = -1
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = rateLimiterStrategies,
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(3, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenRateLimiterStrategiesAreValid()
        {
            var rateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>
            {
                {
                    "Standard-RateLimiter", new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 1,
                        QueueProcessingOrder = QueueProcessingOrder.NewestFirst,
                        QueueLimit = 0
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = rateLimiterStrategies,
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenTotalRequestTimeoutStrategiesAreInvalid()
        {
            // Arrange
            var totalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>
            {
                { "Standard-TotalRequestTimeout", TimeSpan.Zero },
                { "Custom-TotalRequestTimeout", TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1)}
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = totalRequestTimeoutStrategies,
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(2, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTotalRequestTimeoutStrategiesAreValid()
        {
            // Arrange
            var totalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>
            {
                { "Standard-TotalRequestTimeout", TimeSpan.FromMilliseconds(11) }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = totalRequestTimeoutStrategies,
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenRetryStrategiesAreInvalid()
        {
            // Arrange
            var retryStrategies = new Dictionary<string, HttpRetryStrategyOptions>
            {
                {
                    "Standard-Retry", new HttpRetryStrategyOptions
                    {
                        BackoffType = (DelayBackoffType)3,
                        Delay = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1),
                        MaxDelay = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1)
                    }
                },
                {
                    "Custom-Retry", new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        Delay = TimeSpan.FromMicroseconds(-1),
                        MaxDelay = TimeSpan.FromMicroseconds(-1),
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = retryStrategies,
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(5, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenRetryStrategiesAreValid()
        {
            // Arrange
            var retryStrategies = new Dictionary<string, HttpRetryStrategyOptions>
            {
                {
                    "Standard-Retry", new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        Delay = TimeSpan.FromDays(1),
                        MaxDelay = null
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = retryStrategies,
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenCircuitBreakerStrategiesAreInvalid()
        {
            // Arrange
            var circuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>
            {
                {
                    "Standard-CircuitBreaker", new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = 1.1d,
                        MinimumThroughput = 1,
                        SamplingDuration = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1),
                        BreakDuration = TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1),
                    }
                },
                {
                    "Custom-CircuitBreaker", new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = -0.1d,
                        MinimumThroughput = 2, // Valid
                        SamplingDuration = TimeSpan.FromSeconds(0.4),
                        BreakDuration = TimeSpan.FromSeconds(0.4)
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = circuitBreakerStrategies,
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(7, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenCircuitBreakerStrategiesAreValid()
        {
            // Arrange
            var circuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>
            {
                {
                    "Standard-CircuitBreaker", new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = 1d,
                        MinimumThroughput = 2,
                        SamplingDuration = TimeSpan.FromMilliseconds(500),
                        BreakDuration = TimeSpan.FromMilliseconds(500),
                    }
                }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = circuitBreakerStrategies,
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenAttemptTimeoutStrategiesAreInvalid()
        {
            // Arrange
            var attemptTimeoutStrategies = new Dictionary<string, TimeSpan>
            {
                { "Standard-AttemptTimeout", TimeSpan.Zero },
                { "Custom-AttemptTimeout", TimeSpan.FromDays(1) + TimeSpan.FromMicroseconds(1) }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = attemptTimeoutStrategies
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(2, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenAttemptTimeoutStrategiesAreValid()
        {
            // Arrange
            var attemptTimeoutStrategies = new Dictionary<string, TimeSpan>
            {
                { "Standard-AttemptTimeout", TimeSpan.FromMilliseconds(11) }
            };

            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = attemptTimeoutStrategies
            };

            // Act
            var result = ResilienceOptionsValidator.Validate(resilienceOptions);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }
    }
}