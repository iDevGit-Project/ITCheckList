using Microsoft.Extensions.Caching.Memory;

namespace ITCheckList.Services
{
    public class ConnectionAttemptTracker
    {
        private readonly IMemoryCache _cache;
        private const string ATTEMPT_KEY = "ConnectionAttempts";
        private const string LOCKOUT_KEY = "ConnectionLockoutUntil";

        public ConnectionAttemptTracker(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsLockedOut()
        {
            if (_cache.TryGetValue<DateTime>(LOCKOUT_KEY, out var lockoutUntil))
            {
                return DateTime.Now < lockoutUntil;
            }
            return false;
        }

        public void RegisterFailedAttempt()
        {
            int attempts = _cache.GetOrCreate(ATTEMPT_KEY, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return 0;
            });

            attempts++;

            if (attempts >= 3)
            {
                _cache.Set(LOCKOUT_KEY, DateTime.Now.AddMinutes(1), TimeSpan.FromMinutes(2));
                _cache.Remove(ATTEMPT_KEY); // reset attempts
            }
            else
            {
                _cache.Set(ATTEMPT_KEY, attempts, TimeSpan.FromMinutes(5));
            }
        }

        public void Reset()
        {
            _cache.Remove(ATTEMPT_KEY);
            _cache.Remove(LOCKOUT_KEY);
        }
    }

}
