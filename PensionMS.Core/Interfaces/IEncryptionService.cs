namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة التشفير — AES-256 للحقول الحساسة
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
