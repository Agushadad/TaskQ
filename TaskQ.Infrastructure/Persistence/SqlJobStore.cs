using Microsoft.EntityFrameworkCore;
using TaskQ.Core.Abstractions;
using TaskQ.Core.Domain;
using TaskQ.Infrastructure.Persistence;
using TaskQ.Infrastructure.Persistence.Entities;

namespace TaskQ.Infrastructure.Stores
{
    public class SqlJobStore : IJobStore
    {
        #region Propiedades
        private readonly IDbContextFactory<TaskQDbContext> _dbContextFactory;
        #endregion

        #region Constructor
        public SqlJobStore(IDbContextFactory<TaskQDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        #endregion

        #region Metodos heredados
        public async Task EnqueueAsync(Job job, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            var entity = ToEntity(job);

            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            entity.Status = JobStatus.Queued;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.LockedAt = null;
            entity.LockedBy = null;

            db.Jobs.Add(entity);
            await db.SaveChangesAsync(ct);
        }

        public async Task MarkFailedAsync(Guid jobId, string error, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            var job = await db.Jobs.FindAsync(new object[] { jobId }, ct);
            if (job == null)
                return;

            job.Attempts++;
            job.LastError = error;
            job.UpdatedAt = DateTime.UtcNow;
            job.LastExecutionAt = DateTime.UtcNow;

            // Reintento
            if (job.Attempts < job.MaxAttempts)
            {
                job.Status = JobStatus.Queued;
                job.LockedAt = null;
                job.LockedBy = null;
            }
            else
            {
                // Fracaso definitivo
                job.Status = JobStatus.Failed;
                job.CompletedAt = DateTime.UtcNow;
                job.LockedAt = null;
                job.LockedBy = null;
            }

            await db.SaveChangesAsync(ct);
        }

        public async Task MarkSucceededAsync(Guid jobId, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            var job = await db.Jobs.FindAsync(new object[] { jobId }, ct);
            if (job == null)
                return;

            job.Status = JobStatus.Succeeded;
            job.CompletedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            job.LockedAt = null;
            job.LockedBy = null;

            await db.SaveChangesAsync(ct);
        }

        public async Task<Job?> TryAcquireNextJobAsync(string queue, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                var job = await db.Jobs
                    .FromSqlInterpolated($@"
            SELECT TOP(1) *
            FROM Jobs WITH (UPDLOCK, READPAST, ROWLOCK)
            WHERE Queue = {queue}
              AND Status = {(byte)JobStatus.Queued}
              AND (NOT_BEFORE IS NULL OR NOT_BEFORE <= {now})
              AND LOCKED_BY IS NULL
            ORDER BY CREATED_AT")
                    .FirstOrDefaultAsync(ct);

                if (job == null)
                {
                    await tx.CommitAsync(ct);
                    return null;
                }

                job.LockedAt = now;
                job.LockedBy = Environment.MachineName;

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ToDomain(job);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return null;
            }
        }
        #endregion

        #region Metodos Estaticos
        private static Job ToDomain(JobEntity entity)
        {
            return new Job
            {
                Id = entity.Id,
                Type = entity.Type,
                Payload = entity.Payload,
                Status = entity.Status,
                Queue = entity.Queue,
                Attempts = entity.Attempts,
                MaxAttempts = entity.MaxAttempts,
                NotBefore = entity.NotBefore,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                LastExecutionAt = entity.LastExecutionAt,
                CompletedAt = entity.CompletedAt,
                LastError = entity.LastError,
                LockedAt = entity.LockedAt,
                LockedBy = entity.LockedBy
            };
        }
        private static JobEntity ToEntity(Job job)
        {
            return new JobEntity
            {
                Id = job.Id,
                Type = job.Type,
                Payload = job.Payload,
                Status = job.Status,
                Queue = job.Queue,
                Attempts = job.Attempts,
                MaxAttempts = job.MaxAttempts,
                NotBefore = job.NotBefore,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                LastExecutionAt = job.LastExecutionAt,
                CompletedAt = job.CompletedAt,
                LastError = job.LastError,
                LockedAt = job.LockedAt,
                LockedBy = job.LockedBy
            };
        }
        #endregion
    }
}
