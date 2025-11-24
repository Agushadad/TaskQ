using TaskQ.Core.Abstractions;
using TaskQ.Worker.JobHandlers;

namespace TaskQ.Worker
{
    public class JobWorker : BackgroundService
    {
        private readonly IJobStore _jobStore;
        private readonly IJobHandlerResolver _handlerResolver;
        private readonly ILogger<JobWorker> _logger;

        private readonly string _queueName;
        private readonly TimeSpan _idleDelay;

        public JobWorker(IJobStore jobStore, IJobHandlerResolver handlerResolver, ILogger<JobWorker> logger, string queueName = "default", TimeSpan? idleDelay = null)
        {
            _jobStore = jobStore;
            _handlerResolver = handlerResolver;
            _logger = logger;
            _queueName = queueName;
            _idleDelay = idleDelay ?? TimeSpan.FromSeconds(2);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JobWorker iniciado para la cola {Queue}.", _queueName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Intentar adquirir el próximo job
                    var job = await _jobStore.TryAcquireNextJobAsync(_queueName, stoppingToken);

                    if (job == null)
                    {
                        // No hay trabajos → descanso un poco
                        await Task.Delay(_idleDelay, stoppingToken);
                        continue;
                    }

                    _logger.LogInformation("Job {JobId} ({Type}) adquirido.", job.Id, job.Type);

                    // 2. Resolver handler
                    var handler = _handlerResolver.Resolve(job.Type);

                    try
                    {
                        // 3. Ejecutar handler
                        await handler.HandleAsync(job, stoppingToken);

                        // 4. Marcar como exitoso
                        await _jobStore.MarkSucceededAsync(job.Id, stoppingToken);
                        _logger.LogInformation("Job {JobId} procesado con éxito.", job.Id);
                    }
                    catch (Exception ex)
                    {
                        // 5. Marcar como fallido
                        await _jobStore.MarkFailedAsync(job.Id, ex.ToString(), stoppingToken);
                        _logger.LogError(ex, "Error procesando job {JobId}. Marcado como failed/retry.", job.Id);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Cancelación normal del host
                }
                catch (Exception ex)
                {
                    // Error “global” del loop
                    _logger.LogError(ex, "Error en el loop principal del worker.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("JobWorker detenido para la cola {Queue}.", _queueName);
        }
    }
}
