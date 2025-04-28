using Microsoft.Extensions.Http.Resilience;
using Moq;
using Sample.Api.Resilience;
using System.Threading.RateLimiting;

namespace Sample.Api.Tests
{
    public class ResilienceConfigurationValidatorTests
    {
        private readonly Mock<IResilienceConfiguration> _resilienceConfigurationMock;
        private ResilienceConfigurationValidator<IResilienceConfiguration> _validator;

        public ResilienceConfigurationValidatorTests()
        {
            _resilienceConfigurationMock = new Mock<IResilienceConfiguration>();
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenKeyIsEmpty()
        {
            // Arrange
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            _validator = new ResilienceConfigurationValidator<IResilienceConfiguration>(resilienceOptions);

            _resilienceConfigurationMock.Setup(c => c.ResilienceStrategies)
                .Returns(new Dictionary<ResilienceStrategy, string>
                {
                    { ResilienceStrategy.RateLimiter, string.Empty },
                    { ResilienceStrategy.TotalRequestTimeout, string.Empty },
                    { ResilienceStrategy.Retry, string.Empty },
                    { ResilienceStrategy.CircuitBreaker, string.Empty },
                    { ResilienceStrategy.AttemptTimeout, string.Empty }
                });

            // Act
            var result = _validator.Validate(null, _resilienceConfigurationMock.Object);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(5, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenKeyNotFoundInConfig()
        {
            // Arrange
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>()
            };

            _validator = new ResilienceConfigurationValidator<IResilienceConfiguration>(resilienceOptions);

            _resilienceConfigurationMock.Setup(c => c.ResilienceStrategies)
                .Returns(new Dictionary<ResilienceStrategy, string>
                {
                    { ResilienceStrategy.RateLimiter, "Standard-RateLimiter" },
                    { ResilienceStrategy.TotalRequestTimeout, "Standard-TotalRequestTimeout" },
                    { ResilienceStrategy.Retry, "Standard-Retry" },
                    { ResilienceStrategy.CircuitBreaker, "Standard-CircuitBreaker" },
                    { ResilienceStrategy.AttemptTimeout, "Standard-AttemptTimeout" }
                });

            // Act
            var result = _validator.Validate(null, _resilienceConfigurationMock.Object);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(5, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenAttemptTimeoutGreaterThanTotalRequestTimeout()
        {
            // Arrange
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>
                {
                    { "Standard-TotalRequestTimeout", TimeSpan.FromSeconds(5) }
                },
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>(),
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>
                {
                    { "Standard-AttemptTimeout", TimeSpan.FromSeconds(6) }
                }
            };

            _validator = new ResilienceConfigurationValidator<IResilienceConfiguration>(resilienceOptions);

            _resilienceConfigurationMock.Setup(c => c.ResilienceStrategies)
                .Returns(new Dictionary<ResilienceStrategy, string>
                {
                    { ResilienceStrategy.TotalRequestTimeout, "Standard-TotalRequestTimeout" },
                    { ResilienceStrategy.AttemptTimeout, "Standard-AttemptTimeout" }
                });

            // Act
            var result = _validator.Validate(null, _resilienceConfigurationMock.Object);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Single(result.Failures);
        }

        [Fact]
        public void Validate_ShouldReturnError_WhenCircuitBreakerSamplingDurationIsTooShort()
        {
            // Arrange
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>(),
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>(),
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>(),
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>
                {
                    {
                        "Standard-CircuitBreaker", new HttpCircuitBreakerStrategyOptions
                        {
                            SamplingDuration = TimeSpan.FromSeconds(1)
                        }
                    }
                },
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>
                {
                    { "Standard-AttemptTimeout", TimeSpan.FromSeconds(1) }
                }
            };

            _validator = new ResilienceConfigurationValidator<IResilienceConfiguration>(resilienceOptions);

            _resilienceConfigurationMock.Setup(c => c.ResilienceStrategies)
                .Returns(new Dictionary<ResilienceStrategy, string>
                {
                    { ResilienceStrategy.CircuitBreaker, "Standard-CircuitBreaker" },
                    { ResilienceStrategy.AttemptTimeout, "Standard-AttemptTimeout" }
                });

            // Act
            var result = _validator.Validate(null, _resilienceConfigurationMock.Object);

            // Assert
            Assert.True(result.Failed);
            Assert.NotNull(result.Failures);
            Assert.Equal(1, result.Failures.Count());
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenValidConfiguration()
        {
            // Arrange
            var resilienceOptions = new ResilienceOptions
            {
                RateLimiterStrategies = new Dictionary<string, ConcurrencyLimiterOptions>
                {
                    { "Standard-RateLimiter", new ConcurrencyLimiterOptions() }
                },
                TotalRequestTimeoutStrategies = new Dictionary<string, TimeSpan>
                {
                    { "Standard-TotalRequestTimeout", TimeSpan.Zero }
                },
                RetryStrategies = new Dictionary<string, HttpRetryStrategyOptions>
                {
                    { "Standard-Retry", new HttpRetryStrategyOptions() }
                },
                CircuitBreakerStrategies = new Dictionary<string, HttpCircuitBreakerStrategyOptions>
                {
                    { "Standard-CircuitBreaker", new HttpCircuitBreakerStrategyOptions() }
                },
                AttemptTimeoutStrategies = new Dictionary<string, TimeSpan>
                {
                    { "Standard-AttemptTimeout", TimeSpan.Zero }
                }
            };

            _validator = new ResilienceConfigurationValidator<IResilienceConfiguration>(resilienceOptions);

            _resilienceConfigurationMock.Setup(c => c.ResilienceStrategies)
                .Returns(new Dictionary<ResilienceStrategy, string>
                {
                    { ResilienceStrategy.RateLimiter, "Standard-RateLimiter" },
                    { ResilienceStrategy.TotalRequestTimeout, "Standard-TotalRequestTimeout" },
                    { ResilienceStrategy.Retry, "Standard-Retry" },
                    { ResilienceStrategy.CircuitBreaker, "Standard-CircuitBreaker" },
                    { ResilienceStrategy.AttemptTimeout, "Standard-AttemptTimeout" }
                });

            // Act
            var result = _validator.Validate(null, _resilienceConfigurationMock.Object);

            // Assert
            Assert.False(result.Failed);
            Assert.Null(result.Failures);
        }
    }
}