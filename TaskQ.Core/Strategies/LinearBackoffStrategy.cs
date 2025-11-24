using TaskQ.Core.Abstractions;

namespace TaskQ.Core.Strategies
{
    public class LinearBackoffStrategy : IRetryStrategy
    {
        private readonly int _delaySeconds;
        public LinearBackoffStrategy(int delaySeconds = 60)
        {
            _delaySeconds = delaySeconds;
        }
        public TimeSpan GetNextDelay(int attemptNumber)
        {
            return TimeSpan.FromSeconds(_delaySeconds);
        }
    }
}
