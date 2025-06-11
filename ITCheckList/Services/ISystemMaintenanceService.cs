namespace ITCheckList.Services
{
    public interface ISystemMaintenanceService
    {
        void ClearSystemCache();
        bool OptimizeDatabase();
    }
}
