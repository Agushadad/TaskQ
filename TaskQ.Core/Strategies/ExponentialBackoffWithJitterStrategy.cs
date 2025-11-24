using TaskQ.Core.Abstractions;

namespace TaskQ.Core.Strategies
{
    public class ExponentialBackoffWithJitterStrategy : IRetryStrategy
    {
        private readonly int _baseDelaySeconds;
        private readonly int _maxDelaySeconds;
        private readonly Random _random = new Random();

        public ExponentialBackoffWithJitterStrategy(int baseDelaySeconds = 30, int maxDelaySeconds = 3600)
        {
            _baseDelaySeconds = baseDelaySeconds;
            _maxDelaySeconds = maxDelaySeconds;
        }

        public TimeSpan GetNextDelay(int attemptNumber)
        {
            var delaySeconds = _baseDelaySeconds * Math.Pow(2, attemptNumber);
            delaySeconds = Math.Min(delaySeconds, _maxDelaySeconds);
            
            var jitterPercent = _random.NextDouble() * 0.5 - 0.25; // -0.25 a +0.25
            delaySeconds = delaySeconds * (1 + jitterPercent);

            return TimeSpan.FromSeconds(Math.Max(1, delaySeconds));
        }
    }
}
