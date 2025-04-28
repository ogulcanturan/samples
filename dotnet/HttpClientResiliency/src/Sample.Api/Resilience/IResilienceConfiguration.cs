namespace Sample.Api.Resilience
{
public interface IResilienceConfiguration
{
    IReadOnlyDictionary<ResilienceStrategy, string> ResilienceStrategies { get; init; }
}
}