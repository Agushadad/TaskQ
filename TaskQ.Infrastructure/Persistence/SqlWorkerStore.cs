using Microsoft.EntityFrameworkCore;
using TaskQ.Core.Abstractions;
using TaskQ.Infrastructure.Persistence.Entities;

namespace TaskQ.Infrastructure.Persistence
{
    public class SqlWorkerStore : IWorkerStore
    {
        #region Propiedades
        private readonly IDbContextFactory<TaskQDbContext> _dbContextFactory;
        #endregion

        #region Constructor
        public SqlWorkerStore(IDbContextFactory<TaskQDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        #endregion

        #region Metodos heredados
        public async Task HeartBeatAsync(Guid workerId, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            await db.Workers.Where(w => w.WorkerId == workerId).ExecuteUpdateAsync(s => s.SetProperty(w => w.LastSeen, DateTime.UtcNow),ct);
        }

        public async Task<bool> RegisterWorker(Guid workerId, CancellationToken ct)
        {
                await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
                var workerEntity = new WorkerEntity
                {
                    WorkerId = workerId,
                    StartedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };
                await db.Workers.AddAsync(workerEntity);
                return await db.SaveChangesAsync(ct) == 1;
        }
        #endregion
    }
}
