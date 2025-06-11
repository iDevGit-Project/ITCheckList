namespace ITCheckList.Services
{
    public interface IDatabaseConnectionService
    {
        string? GetDecryptedConnectionString();

        bool TestConnection(string connectionString, out string error);

        void SetConnectionString(string connectionString);

        string GetCurrentRawConnectionString();

        bool IsValidFormat(string connectionString);
    }
}
