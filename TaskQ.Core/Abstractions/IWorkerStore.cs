namespace TaskQ.Core.Abstractions
{
    public interface IWorkerStore
    {
        Task HeartBeatAsync(Guid workerId, CancellationToken ct);
        Task<bool> RegisterWorker(Guid workerId, CancellationToken ct);
    }
}
