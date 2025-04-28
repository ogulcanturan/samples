namespace Sample.Api.SampleApiClient
{
    public interface ISampleApiClient
    {
        Task<object[]> GetProductsAsync(CancellationToken cancellationToken = default);
    }
}