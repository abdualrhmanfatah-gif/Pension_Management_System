using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class RankConfiguration : IEntityTypeConfiguration<Rank>
{
    public void Configure(EntityTypeBuilder<Rank> builder)
    {
        builder.ToTable("Ranks");
        builder.HasKey(e => e.RankID);

        builder.Property(e => e.RankName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);

        builder.HasData(
            new Rank { RankID = 1, RankName = "ملازم", RankLevel = 1, BaseSalary = 120000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 2, RankName = "ملازم أول", RankLevel = 2, BaseSalary = 135000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 3, RankName = "نقيب", RankLevel = 3, BaseSalary = 150000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 4, RankName = "رائد", RankLevel = 4, BaseSalary = 165000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 5, RankName = "مقدم", RankLevel = 5, BaseSalary = 180000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 6, RankName = "عقيد", RankLevel = 6, BaseSalary = 200000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 7, RankName = "عميد", RankLevel = 7, BaseSalary = 225000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 8, RankName = "لواء", RankLevel = 8, BaseSalary = 250000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 9, RankName = "فريق", RankLevel = 9, BaseSalary = 280000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 10, RankName = "فريق أول", RankLevel = 10, BaseSalary = 320000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 11, RankName = "جندي", RankLevel = 11, BaseSalary = 65000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 12, RankName = "عريف", RankLevel = 12, BaseSalary = 75000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 13, RankName = "رقيب ثاني", RankLevel = 13, BaseSalary = 85000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 14, RankName = "رقيب اول", RankLevel = 14, BaseSalary = 95000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 15, RankName = "مساعد", RankLevel = 15, BaseSalary = 105000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 16, RankName = "مساعد ثاني", RankLevel = 16, BaseSalary = 110000m, CreatedAt = new DateTime(2024, 1, 1) },
            new Rank { RankID = 17, RankName = "مساعد اول", RankLevel = 17, BaseSalary = 115000m, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
