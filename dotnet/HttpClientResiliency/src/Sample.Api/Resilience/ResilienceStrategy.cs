namespace Sample.Api.Resilience
{
    public enum ResilienceStrategy
    {
        RateLimiter = 1,
        TotalRequestTimeout = 2,
        Retry = 3,
        CircuitBreaker = 4,
        AttemptTimeout = 5
    }
}