using TaskQ.Core.Abstractions;
using TaskQ.Worker.JobHandlers;

namespace TaskQ.Worker
{
    public class JobWorker : BackgroundService
    {
        private readonly IJobStore _jobStore;
        private readonly IWorkerStore _workerStore;
        private readonly IJobHandlerResolver _handlerResolver;
        private readonly ILogger<JobWorker> _logger;
        private readonly Guid _workerId = Guid.NewGuid();
        private readonly string _queueName;
        private readonly TimeSpan _idleDelay;
        private bool _registered = false;
        private Task? _heartbeatTask;

        public JobWorker(IJobStore jobStore, IWorkerStore workerStore, IJobHandlerResolver handlerResolver, ILogger<JobWorker> logger, string queueName = "default", TimeSpan? idleDelay = null)
        {
            _jobStore = jobStore;
            _handlerResolver = handlerResolver;
            _logger = logger;
            _queueName = queueName;
            _idleDelay = idleDelay ?? TimeSpan.FromSeconds(2);
            _workerStore = workerStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JobWorker iniciado para la cola {Queue}.", _queueName);

            if (!_registered)
            {
                await _workerStore.RegisterWorker(_workerId, stoppingToken);
                _registered = true;
                _heartbeatTask = RunHeartbeatLoopAsync(stoppingToken);
            }

            await _workerStore.HeartBeatAsync(_workerId, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Intentar adquirir el próximo job
                    var job = await _jobStore.TryAcquireNextJobAsync(_queueName, _workerId, stoppingToken);

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
                    _logger.LogInformation("JobWorker cancelado para la cola {Queue}.", _queueName);
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_heartbeatTask != null)
            {
                try
                {
                    await _heartbeatTask;
                }
                catch (OperationCanceledException)
                {
                    // Cancellation expected on shutdown
                }
            }
        }
        private async Task RunHeartbeatLoopAsync(CancellationToken ct)
        {
            _logger.LogInformation("Heartbeat iniciado para worker {WorkerId}", _workerId);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await _workerStore.HeartBeatAsync(_workerId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en heartbeat del worker {WorkerId}", _workerId);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Heartbeat detenido para worker {WorkerId}", _workerId);
        }
    }
}
