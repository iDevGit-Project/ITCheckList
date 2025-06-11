namespace ITCheckList.Services
{
    public interface ILogService
    {
        Task LogAsync(string controller, string action, string description, string userName = "ناشناس", string entityId = "");
    }

}
