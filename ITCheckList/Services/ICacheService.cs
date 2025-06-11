namespace ITCheckList.Services
{
    public interface ICacheService
    {
        void SetWithTracking<T>(string key, T value, TimeSpan duration);
        T Get<T>(string key);
        void Remove(string key);
        bool Exists(string key);
    }

}
