namespace TaskBoard.Infrastructure.Events;

public interface IBackgroundTaskExecutor
{
    void Execute(Func<IServiceProvider, Task> work);
}
