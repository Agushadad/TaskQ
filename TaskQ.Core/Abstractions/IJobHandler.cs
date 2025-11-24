using TaskQ.Core.Domain;

namespace TaskQ.Core.Abstractions
{
    public interface IJobHandler
    {
        Task ExecuteAsync(JobContext context);
    }
}
