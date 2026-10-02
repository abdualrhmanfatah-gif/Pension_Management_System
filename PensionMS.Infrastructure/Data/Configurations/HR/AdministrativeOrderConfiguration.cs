using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Infrastructure.Data.Configurations.HR;

public class AdministrativeOrderConfiguration : IEntityTypeConfiguration<AdministrativeOrder>
{
    public void Configure(EntityTypeBuilder<AdministrativeOrder> builder)
    {
        builder.ToTable("AdministrativeOrders");
        builder.HasKey(o => o.AdministrativeOrderID);

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Subject).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Status).HasMaxLength(50).IsRequired().HasDefaultValue("Draft");
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.HasOne(o => o.Employee)
            .WithMany()
            .HasForeignKey(o => o.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.AdministrativeOrderType)
            .WithMany(t => t.AdministrativeOrders)
            .HasForeignKey(o => o.AdministrativeOrderTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ApprovedByUser)
            .WithMany()
            .HasForeignKey(o => o.ApprovedByUserID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.OrderNumber).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.EmployeeID);
        builder.HasIndex(o => o.AdministrativeOrderTypeID);
        builder.HasIndex(o => o.Status);
    }
}
