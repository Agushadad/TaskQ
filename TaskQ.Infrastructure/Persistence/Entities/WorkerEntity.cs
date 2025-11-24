namespace TaskQ.Infrastructure.Persistence.Entities
{
    public class WorkerEntity
    {  
        public Guid WorkerId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
