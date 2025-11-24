using TaskQ.Core.Domain;

namespace TaskQ.Core.Abstractions
{
    public interface IJobStore
    {
        /// <summary>
        /// Guarda el job en la BD
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task EnqueueAsync(Job job, CancellationToken ct);
        /// <summary>
        /// Obtiene el siguiente job disponible en la cola indicada y lo bloquea para su procesamiento.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Job?> TryAcquireNextJobAsync(string queue, Guid workerId, CancellationToken ct);
        /// <summary>
        /// Marca un job como completado
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task MarkSucceededAsync(Guid jobId, CancellationToken ct);
        /// <summary>
        /// Marca un job como fallido
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="error"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task MarkFailedAsync(Guid jobId, string error, CancellationToken ct);
    }
}
