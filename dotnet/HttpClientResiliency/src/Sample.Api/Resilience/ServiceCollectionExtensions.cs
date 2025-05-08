using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimiting;

namespace Sample.Api.Resilience
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResilienceOptions(this IServiceCollection services,
            IConfiguration configuration)
        {
            var resilienceOptionsSection = configuration.GetSection(nameof(ResilienceOptions));

            if (!resilienceOptionsSection.Exists())
            {
                throw new InvalidOperationException($"The 'appsettings.json' does not contain a section with the name '{nameof(ResilienceOptions)}'.");
            }

            services.AddOptions<ResilienceOptions>().Bind(resilienceOptionsSection).ValidateOnStart();
            services.AddSingleton<ResilienceOptions>(sp => sp.GetRequiredService<IOptions<ResilienceOptions>>().Value);

            return services;
        }

        public static IHttpClientBuilder AddCustomResilienceHandler<TResilienceConfiguration>(this IHttpClientBuilder builder, TResilienceConfiguration resilienceConfiguration) where TResilienceConfiguration : class, IResilienceConfiguration
        {
            var hasRateLimiterStrategy = resilienceConfiguration.ResilienceStrategies.TryGetValue(ResilienceStrategy.RateLimiter, out var rateLimiterStrategyKey) && !string.IsNullOrWhiteSpace(rateLimiterStrategyKey);
            var hasTotalRequestTimeoutStrategy = resilienceConfiguration.ResilienceStrategies.TryGetValue(ResilienceStrategy.TotalRequestTimeout, out var totalRequestTimeoutStrategyKey) && !string.IsNullOrWhiteSpace(totalRequestTimeoutStrategyKey);
            var hasRetryStrategy = resilienceConfiguration.ResilienceStrategies.TryGetValue(ResilienceStrategy.Retry, out var retryStrategyKey) && !string.IsNullOrWhiteSpace(retryStrategyKey);
            var hasCircuitBreakerStrategy = resilienceConfiguration.ResilienceStrategies.TryGetValue(ResilienceStrategy.CircuitBreaker, out var circuitBreakerStrategyKey) && !string.IsNullOrWhiteSpace(circuitBreakerStrategyKey);
            var hasAttemptTimeoutStrategy = resilienceConfiguration.ResilienceStrategies.TryGetValue(ResilienceStrategy.AttemptTimeout, out var attemptTimeoutStrategyKey) && !string.IsNullOrWhiteSpace(attemptTimeoutStrategyKey);

            builder.Services.AddSingleton<IValidateOptions<TResilienceConfiguration>, ResilienceConfigurationValidator<TResilienceConfiguration>>();

            const string pipeline = "CustomResilienceHandler";
            var resilienceHandlerLoggerCategoryName = $"{builder.Name}-{pipeline}";

            builder.AddResilienceHandler(pipeline, (resiliencePipelineBuilder, context) =>
            {
                // context.EnableReloads<ResilienceOptions>(); // Use with caution, changing the configuration key (e.g. Standard-RateLimiter => Standard-RateLimiter2) may cause unexpected issues.

                var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(resilienceHandlerLoggerCategoryName);

                var resilienceOptions = context.ServiceProvider.GetRequiredService<IOptionsMonitor<ResilienceOptions>>().CurrentValue;

                if (hasRateLimiterStrategy)
                {
                    var rateLimiter = new RateLimiterStrategyOptions
                    {
                        Name = rateLimiterStrategyKey,
                        DefaultRateLimiterOptions = resilienceOptions.RateLimiterStrategies[rateLimiterStrategyKey!],
                    };

                    rateLimiter.HandleRateLimiterActions(logger, builder.Name);

                    resiliencePipelineBuilder.AddRateLimiter(rateLimiter);
                }

                if (hasTotalRequestTimeoutStrategy && resilienceOptions.TotalRequestTimeoutStrategies.TryGetValue(totalRequestTimeoutStrategyKey!, out var totalRequestTimeout))
                {
                    var timeout = new HttpTimeoutStrategyOptions
                    {
                        Name = totalRequestTimeoutStrategyKey,
                        Timeout = totalRequestTimeout
                    };

                    timeout.HandleTimeoutActions(logger, builder.Name, nameof(ResilienceStrategy.TotalRequestTimeout));

                    resiliencePipelineBuilder.AddTimeout(timeout);
                }

                if (hasRetryStrategy && resilienceOptions.RetryStrategies.TryGetValue(retryStrategyKey!, out var retryStrategy))
                {
                    retryStrategy.Name = retryStrategyKey;

                    retryStrategy.HandleRetryActions(logger, builder.Name);

                    resiliencePipelineBuilder.AddRetry(retryStrategy);
                }

                if (hasCircuitBreakerStrategy && resilienceOptions.CircuitBreakerStrategies.TryGetValue(circuitBreakerStrategyKey!, out var circuitBreaker))
                {
                    circuitBreaker.Name = circuitBreakerStrategyKey;

                    circuitBreaker.HandleCircuitBreakerActions(logger, builder.Name);

                    resiliencePipelineBuilder.AddCircuitBreaker(circuitBreaker);
                }

                if (hasAttemptTimeoutStrategy && resilienceOptions.AttemptTimeoutStrategies.TryGetValue(attemptTimeoutStrategyKey!, out var attemptTimeout))
                {
                    var timeout = new HttpTimeoutStrategyOptions
                    {
                        Name = attemptTimeoutStrategyKey,
                        Timeout = attemptTimeout
                    };

                    timeout.HandleTimeoutActions(logger, builder.Name, nameof(ResilienceStrategy.AttemptTimeout));

                    resiliencePipelineBuilder.AddTimeout(timeout);
                }
            });

            if (hasTotalRequestTimeoutStrategy || hasAttemptTimeoutStrategy)
            {
                builder.ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan);
            }

            return builder;
        }
    }
}