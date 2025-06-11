namespace ITCheckList.Services
{
    public interface IConnectionSecurityService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        bool IsEncrypted(string input);
    }

}
