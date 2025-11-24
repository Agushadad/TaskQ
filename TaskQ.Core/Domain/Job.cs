namespace TaskQ.Core.Domain
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Type { get; set; }        
        public string? Payload { get; set; }
        public JobStatus Status { get; set; }
        public string Queue { get; set; }
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
