namespace TaskQ.Core.Abstractions
{
    public interface IBackgroundJobClient
    {
        Task EnqueueAsync<THandler>(object payload, CancellationToken ct) where THandler : IJobHandler;
    }
}
