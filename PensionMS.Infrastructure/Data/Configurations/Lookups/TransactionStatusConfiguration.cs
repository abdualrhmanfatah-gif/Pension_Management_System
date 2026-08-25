using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class TransactionStatusConfiguration : IEntityTypeConfiguration<TransactionStatus>
{
    public void Configure(EntityTypeBuilder<TransactionStatus> builder)
    {
        builder.HasKey(e => e.StatusID);

        builder.Property(e => e.StatusName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StatusColor).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasData(
            new TransactionStatus { StatusID = 1, StatusName = "قيد الإرسال", StatusColor = "warning", Description = "تم إنشاء الحوالة ولم ترسل بعد" },
            new TransactionStatus { StatusID = 2, StatusName = "تم الإرسال", StatusColor = "info", Description = "تم إرسال الحوالة للبنك" },
            new TransactionStatus { StatusID = 3, StatusName = "تم الاستلام", StatusColor = "success", Description = "تم استلام الحوالة من المستفيد" },
            new TransactionStatus { StatusID = 4, StatusName = "فشل", StatusColor = "destructive", Description = "فشلت الحوالة" },
            new TransactionStatus { StatusID = 5, StatusName = "ملغي", StatusColor = "secondary", Description = "تم إلغاء الحوالة" }
        );
    }
}
