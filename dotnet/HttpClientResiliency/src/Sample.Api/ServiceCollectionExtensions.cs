using Sample.Api.Resilience;
using Sample.Api.SampleApi;
using Sample.Api.SampleApiClient;

namespace Sample.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSampleApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            var sampleApiHttpClientConfigurationSection = configuration.GetSection(nameof(SampleApiClientConfiguration));

            if (!sampleApiHttpClientConfigurationSection.Exists())
            {
                throw new InvalidOperationException($"The 'appsettings.json' does not contain a section with the name '{nameof(SampleApiClientConfiguration)}'.");
            }

            var sampleApiHttpClientConfiguration = sampleApiHttpClientConfigurationSection.Get<SampleApiClientConfiguration>() ?? throw new InvalidOperationException($"Failed to bind configuration for '{nameof(SampleApiClientConfiguration)}.'");

            if (!Uri.TryCreate(sampleApiHttpClientConfiguration.Url, UriKind.RelativeOrAbsolute, out var sampleApiHttpClientUri))
            {
                throw new Exception($"{nameof(sampleApiHttpClientConfiguration)}.{nameof(sampleApiHttpClientConfiguration.Url)} is not valid.");
            }

            services.AddOptions<SampleApiClientConfiguration>().Bind(sampleApiHttpClientConfigurationSection).ValidateOnStart();

            // Adding named http client 'SampleApiHttpClient'
            services.AddHttpClient(nameof(SampleApiClient), configureClient: httpClient =>
            {
                httpClient.BaseAddress = sampleApiHttpClientUri;
            }).AddCustomResilienceHandler(sampleApiHttpClientConfiguration);

            services.AddSingleton<ISampleApiClient, SampleApiClient.SampleApiClient>();

            return services;
        }
    }
}