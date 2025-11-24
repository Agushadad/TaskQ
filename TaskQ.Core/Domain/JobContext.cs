namespace TaskQ.Core.Domain
{
    public class JobContext
    {
        public Job Job { get; }
        public CancellationToken CancellationToken { get; }

        public JobContext(Job job, CancellationToken cancellationToken)
        {
            Job = job;
            CancellationToken = cancellationToken;
        }
    }
}
