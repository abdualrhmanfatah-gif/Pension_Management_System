using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Data;

/// <summary>
/// EF Core Value Converter — يشفّر القيمة قبل حفظها ويفكها عند قراءتها
/// يُستخدم مع الحقول الحساسة مثل أرقام الحسابات البنكية و IBAN
/// </summary>
public class EncryptedValueConverter : ValueConverter<string, string>
{
    public EncryptedValueConverter(IEncryptionService encryptionService)
        : base(
            v => encryptionService.Encrypt(v),
            v => encryptionService.Decrypt(v))
    {
    }
}
