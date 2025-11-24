using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskQ.Core.Abstractions;
using TaskQ.Core.Strategies;
using TaskQ.Infrastructure.Persistence;
using TaskQ.Infrastructure.Stores;
using TaskQ.Worker;
using TaskQ.Worker.JobHandlers;
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // DbContext factory
        services.AddPooledDbContextFactory<TaskQDbContext>(options =>
         options.UseSqlServer(configuration.GetConnectionString("TaskQ")));

        // Store
        services.AddSingleton<IJobStore, SqlJobStore>();
        services.AddSingleton<IWorkerStore, SqlWorkerStore>();
        services.AddSingleton<IRetryStrategy>(sp => new ExponentialBackoffStrategy(
              baseDelaySeconds: 30,    // Primer reintento: 30s
              maxDelaySeconds: 3600    // Máximo: 1 hora
        ));
        // Handlers
        services.AddTransient<TaskQ.Worker.JobHandlers.IJobHandler, SendEmailJobHandler>();

        // services.AddTransient<IJobHandler, OtroJobHandler>();
        // etc...

        // Resolver
        services.AddSingleton<IJobHandlerResolver, JobHandlerResolver>();
        services.AddHostedService<DemoJobSeeder>();
        // Worker
        services.AddHostedService<JobWorker>();
    });

await builder.RunConsoleAsync();