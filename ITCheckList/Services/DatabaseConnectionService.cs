using ITCheckList.Helpers;
using Microsoft.Data.SqlClient;

namespace ITCheckList.Services
{
    public class DatabaseConnectionService : IDatabaseConnectionService
    {
        private readonly IConfiguration _configuration;
        private string _currentConnectionString;

        public DatabaseConnectionService(IConfiguration configuration)
        {
            _configuration = configuration;
            _currentConnectionString = configuration.GetConnectionString("StrConDb") ?? string.Empty;
        }

        public string? GetDecryptedConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_currentConnectionString))
                return null;

            try
            {
                if (_currentConnectionString.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                {
                    var base64Part = _currentConnectionString.Substring(4);

                    // بررسی صحت Base64
                    Convert.FromBase64String(base64Part);

                    if (EncryptHelper.TryDecrypt(base64Part, out string decrypted))
                    {
                        return decrypted;
                    }
                    else
                    {
                        throw new Exception("رشته اتصال رمزگشایی نشد یا قالب آن معتبر نیست.");
                    }
                }
                else
                {
                    return _currentConnectionString;
                }
            }
            catch
            {
                return null;
            }
        }

        public bool TestConnection(string connectionString, out string error)
        {
            error = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch (SqlException sqlEx)
            {
                error = $"خطای دیتابیس: {sqlEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"خطا: {ex.Message}";
                return false;
            }
        }

        public void SetConnectionString(string connectionString)
        {
            _currentConnectionString = connectionString;
        }

        public string GetCurrentRawConnectionString()
        {
            return _currentConnectionString;
        }

        public bool IsValidFormat(string connectionString)
        {
            return connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
                && connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase);
        }
    }

}
