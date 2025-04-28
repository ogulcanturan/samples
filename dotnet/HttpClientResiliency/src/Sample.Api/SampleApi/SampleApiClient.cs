namespace Sample.Api.SampleApiClient
{
    public class SampleApiClient(IHttpClientFactory httpClientFactory) : ISampleApiClient
    {
        public async Task<object[]> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            var httpClient = GetHttpClient();

            using var response = await httpClient.GetAsync(RelativeUris.GetProducts, cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<object[]>(cancellationToken);
        }

        private HttpClient GetHttpClient()
        {
            return httpClientFactory.CreateClient(nameof(SampleApiClient));
        }

        private static class RelativeUris
        {
            public const string GetProducts = "products";
        }
    }
}