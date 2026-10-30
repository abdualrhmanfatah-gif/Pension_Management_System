using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations.Lookups;

public class FileStatusConfiguration : IEntityTypeConfiguration<FileStatus>
{
    public void Configure(EntityTypeBuilder<FileStatus> builder)
    {
        builder.HasKey(e => e.StatusID);

        builder.Property(e => e.StatusName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StatusColor).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasData(
            new FileStatus { StatusID = 1, StatusName = "جديد", StatusColor = "outline", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 2, StatusName = "نشط", StatusColor = "success", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
<<<<<<< HEAD
            new FileStatus { StatusID = 4, StatusName = "انتهى استحقاقه", StatusColor = "secondary", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 6, StatusName = "معلق", StatusColor = "warning", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
=======
            new FileStatus { StatusID = 3, StatusName = "موقوف", StatusColor = "destructive", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 4, StatusName = "مغلق", StatusColor = "secondary", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 5, StatusName = "ناقص", StatusColor = "warning", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 6, StatusName = "معلق", StatusColor = "warning", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 7, StatusName = "تحت المراجعة", StatusColor = "info", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new FileStatus { StatusID = 8, StatusName = "مرفوض", StatusColor = "destructive", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        );
    }
}
