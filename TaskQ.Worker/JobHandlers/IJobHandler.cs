using TaskQ.Core.Domain;

namespace TaskQ.Worker.JobHandlers
{
    public interface IJobHandler
    {      
        Task HandleAsync(Job job, CancellationToken ct);
    }
}
