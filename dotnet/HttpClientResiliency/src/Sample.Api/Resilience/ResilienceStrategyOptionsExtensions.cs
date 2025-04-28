using Microsoft.Extensions.Http.Resilience;
using Polly.RateLimiting;

namespace Sample.Api.Resilience
{
    internal static class ResilienceStrategyOptionsExtensions
    {
        private const string ReasonsJoiner = ", ";

        public static void HandleRateLimiterActions(this RateLimiterStrategyOptions options, ILogger logger, string builderName)
        {
            options.OnRejected = rejected =>
            {
                if (rejected.Lease.IsAcquired)
                {
                    return ValueTask.CompletedTask;
                }

                var reasons = string.Join(ReasonsJoiner, rejected.Lease.MetadataNames
                    .Select(metadataName => rejected.Lease.TryGetMetadata(metadataName, out var reason) ? reason : null)
                    .Where(r => r != null));

                Logs.HandleRateLimiterOnRejectedError(logger, nameof(ResilienceStrategy.RateLimiter), reasons, null);

                return ValueTask.CompletedTask;
            };
        }

        public static void HandleCircuitBreakerActions(this HttpCircuitBreakerStrategyOptions options, ILogger logger, string builderName)
        {
            options.OnClosed = _ =>
            {
                Logs.HandleCircuitBreakerOnClosedInfo(logger, nameof(ResilienceStrategy.CircuitBreaker), null);

                return ValueTask.CompletedTask;
            };

            options.OnOpened = open =>
            {
                if (open.Outcome.Exception != null)
                {
                    Logs.HandleCircuitBreakerOnOpenedActionError(logger, nameof(ResilienceStrategy.CircuitBreaker), open.Outcome.Exception.Message, options.BreakDuration.TotalSeconds, open.Outcome.Exception);
                }

                return ValueTask.CompletedTask;
            };
        }

        public static void HandleRetryActions(this HttpRetryStrategyOptions options, ILogger logger, string builderName)
        {
            options.OnRetry = retry =>
            {
                if (retry.Outcome.Exception != null)
                {
                    Logs.HandleRetryActionsError(logger, nameof(ResilienceStrategy.Retry), retry.Outcome.Exception.Message, retry.AttemptNumber + 1, retry.Outcome.Exception);
                }

                return ValueTask.CompletedTask;
            };
        }

        private static class Logs
        {
            public static readonly Action<ILogger, string, string, Exception> HandleRateLimiterOnRejectedError = LoggerMessage.Define<string, string>(LogLevel.Error, 1, "{retryStrategyName} - Request rejected. Reason(s): {reasons}");

            public static readonly Action<ILogger, string, Exception> HandleCircuitBreakerOnClosedInfo = LoggerMessage.Define<string>(LogLevel.Information, 2, "{retryStrategyName} - Circuit is now closed and connection re-established.");

            public static readonly Action<ILogger, string, string, double, Exception> HandleCircuitBreakerOnOpenedActionError = LoggerMessage.Define<string, string, double>(LogLevel.Error, 3, "{retryStrategyName} - Circuit broken due to exception. Message: {message}, will reset in {breakDurationInSeconds} seconds.");

            public static readonly Action<ILogger, string, string, int, Exception> HandleRetryActionsError = LoggerMessage.Define<string, string, int>(LogLevel.Error, 4, "{retryStrategyName} - Failed due to exception. Message: {message}, Attempt: {attemptNumber}.");
        }
    }
}