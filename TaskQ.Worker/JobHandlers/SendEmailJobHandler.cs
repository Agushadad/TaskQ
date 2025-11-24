using System.Text.Json;
using TaskQ.Core.Attributes;
using TaskQ.Core.Domain;

namespace TaskQ.Worker.JobHandlers
{
    [BackgroundJob("SendEmailJob")]
    public class SendEmailJobHandler : IJobHandler
    {    
        public async Task HandleAsync(Job job, CancellationToken ct)
        {         
            var payload = JsonSerializer.Deserialize<SendEmailPayload>(job.Payload);

            if (payload == null)
                throw new InvalidOperationException("Payload de SendEmail es nulo o inválido.");

            await Task.Delay(100, ct);
        }
    }

    public record SendEmailPayload(
        string To,
        string Subject,
        string Body
    );
}
