using TaskQ.Core.Domain;

namespace TaskQ.Infrastructure.Persistence.Entities
{
    public class JobEntity
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string? Payload { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Queued;
        public string Queue { get; set; } = "default";
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastExecutionAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? LastError { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedBy { get; set; }
    }
}
