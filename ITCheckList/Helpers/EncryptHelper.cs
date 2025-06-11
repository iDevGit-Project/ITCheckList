using System.Security.Cryptography;
using System.Text;

public static class EncryptHelper
{
    // ✅ کلید قوی و ثابت — بهتر است از appsettings یا Secret Store خوانده شود
    private static readonly string EncryptionKey = "ReplaceWithYourStrongKey123!"; // ← در محیط production حتما قوی و تصادفی باشد

    /// <summary>
    /// رمزنگاری یک رشته به Base64 با AES
    /// </summary>
    public static string Encrypt(string plainText)
    {
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV(); // IV تصادفی برای هر بار رمزنگاری

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] combinedBytes = aes.IV.Concat(cipherBytes).ToArray();
        return Convert.ToBase64String(combinedBytes);
    }

    /// <summary>
    /// رمزگشایی رشته رمزنگاری‌شده Base64
    /// </summary>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            throw new ArgumentException("Cipher text is null or empty.");

        byte[] combinedBytes = Convert.FromBase64String(cipherText);
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));

        using var aes = Aes.Create();
        aes.Key = key;

        byte[] iv = combinedBytes.Take(aes.BlockSize / 8).ToArray();
        byte[] cipherBytes = combinedBytes.Skip(aes.BlockSize / 8).ToArray();

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// تلاش برای رمزگشایی → رشته Base64 نامعتبر یا رشته خراب → false بدون Exception
    /// </summary>
    public static bool TryDecrypt(string cipherText, out string plainText)
    {
        plainText = string.Empty;

        if (string.IsNullOrWhiteSpace(cipherText))
            return false;

        byte[] combinedBytes;

        try
        {
            combinedBytes = Convert.FromBase64String(cipherText);
        }
        catch
        {
            return false; // رشته Base64 نیست
        }

        try
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(EncryptionKey));
            using var aes = Aes.Create();
            aes.Key = key;

            byte[] iv = combinedBytes.Take(aes.BlockSize / 8).ToArray();
            byte[] cipherBytes = combinedBytes.Skip(aes.BlockSize / 8).ToArray();

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            plainText = Encoding.UTF8.GetString(plainBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
