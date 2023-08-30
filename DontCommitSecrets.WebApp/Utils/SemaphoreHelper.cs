namespace DontCommitSecrets.WebApp.Utils;

public class SemaphoreHelper
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task Run(Func<Task> task)
    {
        await Run(async () =>
        {
            await task.Invoke();
            return true;
        });
    }

    public async Task<T> Run<T>(Func<Task<T>> task)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await task.Invoke();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
