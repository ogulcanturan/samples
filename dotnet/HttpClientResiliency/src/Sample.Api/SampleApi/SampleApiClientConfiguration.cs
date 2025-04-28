using Sample.Api.Resilience;
using System.Collections.ObjectModel;

namespace Sample.Api.SampleApi
{
    public class SampleApiClientConfiguration : IResilienceConfiguration
    {
        public required string Url { get; init; }

        public IReadOnlyDictionary<ResilienceStrategy, string> ResilienceStrategies { get; init; } =
            new ReadOnlyDictionary<ResilienceStrategy, string>(new Dictionary<ResilienceStrategy, string>());
    }
}