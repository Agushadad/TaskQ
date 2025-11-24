using Microsoft.Extensions.Hosting;
using TaskQ.Core.Abstractions;
using TaskQ.Core.Domain;

public class DemoJobSeeder : IHostedService
{
    private readonly IJobStore _jobStore;

    public DemoJobSeeder(IJobStore jobStore)
    {
        _jobStore = jobStore;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("🌱 Encolando job de prueba...");

        var job = new Job
        {
            // El Id se asigna en el store si viene vacío
            Type = "SendEmailJob",
            Payload = "{ \"to\": \"test@example.com\", \"subject\": \"Hola\", \"body\": \"Probando TaskQ\" }",
            MaxAttempts = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow
        };

        await _jobStore.EnqueueAsync(job, cancellationToken);

        Console.WriteLine("🌱 Job encolado correctamente.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
