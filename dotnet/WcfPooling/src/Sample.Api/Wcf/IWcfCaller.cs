namespace Sample.Api.Wcf;

public interface IWcfCaller<out TContract>
{
    TResult Call<TResult>(Func<TContract, TResult> call);

    Task<TResult> CallAsync<TResult>(Func<TContract, Task<TResult>> call);
}