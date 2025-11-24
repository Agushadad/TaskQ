namespace TaskQ.Core.Domain
{
    public enum JobStatus
    {
        Queued = 0,
        Processing = 1,
        Succeeded = 2,
        Failed = 3,
        DeadLetter = 4
    }
}
