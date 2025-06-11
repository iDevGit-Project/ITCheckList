using System.Security.Cryptography;
using System.Text;

namespace ITCheckList.Services
{
    public class ConnectionSecurityService : IConnectionSecurityService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public ConnectionSecurityService(IConfiguration configuration)
        {
            // دریافت کلید و IV از فایل تنظیمات یا متغیر محیطی
            _key = Convert.FromBase64String(configuration["Encryption:Key"]);
            _iv = Convert.FromBase64String(configuration["Encryption:IV"]);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var cipherBytes = Convert.FromBase64String(cipherText);
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (FormatException)
            {
                throw new Exception("رشته رمزگذاری شده معتبر نیست. احتمالاً رشته قبلاً رمزنگاری نشده است یا فرمت اشتباه دارد.");
            }
            catch (CryptographicException)
            {
                throw new Exception("رشته رمزگذاری شده نامعتبر است یا رمزگشایی ممکن نیست.");
            }
        }

        public bool IsEncrypted(string input)
        {
            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
