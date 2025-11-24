namespace TaskQ.Core.Abstractions
{
    public interface IRetryStrategy
    {
        TimeSpan GetNextDelay(int attemptNumber);
    }
}
