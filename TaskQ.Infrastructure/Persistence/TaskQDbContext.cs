using Microsoft.EntityFrameworkCore;
using TaskQ.Infrastructure.Persistence.Entities;

namespace TaskQ.Infrastructure.Persistence
{
    public class TaskQDbContext : DbContext
    {
        public TaskQDbContext(DbContextOptions<TaskQDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobEntity> Jobs => Set<JobEntity>();
        public DbSet<WorkerEntity> Workers => Set<WorkerEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var job = modelBuilder.Entity<JobEntity>();

            job.ToTable("Jobs", "dbo");

            job.HasKey(j => j.Id);

            job.Property(j => j.Id)
                .HasColumnName("ID")
                .IsRequired();

            job.Property(j => j.Type)
                .HasColumnName("TYPE")
                .HasMaxLength(200)
                .IsRequired();

            job.Property(j => j.Payload)
                .HasColumnName("PAYLOAD");

            job.Property(j => j.Status)
                .HasColumnName("STATUS")
                .IsRequired();

            job.Property(j => j.Queue)
                .HasColumnName("QUEUE")
                .HasMaxLength(50)
                .HasDefaultValue("default")
                .IsRequired();

            job.Property(j => j.Attempts)
                .HasColumnName("ATTEMPTS");

            job.Property(j => j.MaxAttempts)
                .HasColumnName("MAX_ATTEMPTS");

            job.Property(j => j.NotBefore)
                .HasColumnName("NOT_BEFORE");

            job.Property(j => j.CreatedAt)
                .HasColumnName("CREATED_AT");

            job.Property(j => j.UpdatedAt)
                .HasColumnName("UPDATED_AT");

            job.Property(j => j.LastExecutionAt)
                .HasColumnName("LAST_EXECUTION_AT");

            job.Property(j => j.CompletedAt)
                .HasColumnName("COMPLETED_AT");

            job.Property(j => j.LastError)
                .HasColumnName("LAST_ERROR");

            job.Property(j => j.LockedAt)
                .HasColumnName("LOCKED_AT");

            job.Property(j => j.LockedBy)
                .HasColumnName("LOCKED_BY")
                .HasMaxLength(100);

            job.Property(j => j.LockedUntil)
              .HasColumnName("LOCKED_UNTIL");

            // Índice clave para TryAcquireNextJobAsync
            job.HasIndex(j => new { j.Status, j.NotBefore, j.Queue })
               .HasDatabaseName("IX_Jobs_Status_NotBefore_Queue");

            var worker = modelBuilder.Entity<WorkerEntity>();
            worker.ToTable("Workers", "dbo");

            worker.HasKey(w => w.WorkerId);
            worker.Property(w => w.WorkerId)
                .HasColumnName("ID")
                .IsRequired();

            worker.Property(w => w.LastSeen)
                .HasColumnName("LAST_SEEN")
                .IsRequired();

            worker.Property(w => w.StartedAt)
                .HasColumnName("STARTED_AT")
                .IsRequired();
        }
    }
}
