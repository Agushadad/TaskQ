namespace TaskQ.Core.Domain
{
    public class Worker
    {
        public Guid WorkerId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
