using TaskQ.Core.Abstractions;

namespace TaskQ.Core.Strategies
{
    public class ExponentialBackoffStrategy : IRetryStrategy
    {
        private readonly int _baseDelaySeconds;
        private readonly int _maxDelaySeconds;
        public ExponentialBackoffStrategy(int baseDelaySeconds = 30, int maxDelaySeconds = 3600)
        {
            _baseDelaySeconds = baseDelaySeconds;
            _maxDelaySeconds = maxDelaySeconds;
        }
        public TimeSpan GetNextDelay(int attemptNumber)
        {
            var delaySeconds = _baseDelaySeconds * Math.Pow(2, attemptNumber);            
            delaySeconds = Math.Min(delaySeconds, _maxDelaySeconds);
            return TimeSpan.FromSeconds(delaySeconds);
        }
    }
}
