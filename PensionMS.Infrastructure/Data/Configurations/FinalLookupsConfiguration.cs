using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class PaymentBookConfiguration : IEntityTypeConfiguration<PaymentBook>
{
    public void Configure(EntityTypeBuilder<PaymentBook> builder)
    {
        builder.HasKey(e => e.PaymentBookID);

        builder.Property(e => e.BookNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.Property(e => e.BookType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.LegacyColor).HasMaxLength(20);
        builder.Property(e => e.ChangeReason).HasMaxLength(100);
        builder.Property(e => e.RecipientName).HasMaxLength(200);
        builder.Property(e => e.RecipientType).HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.Property(e => e.PendingReviewStatus)
            .HasConversion<int>()
            .HasDefaultValue(0);

        builder.HasOne(pb => pb.RetirementFile).WithMany(f => f.PaymentBooks).HasForeignKey(pb => pb.FileID);
        builder.HasOne(pb => pb.IssuingBranch).WithMany().HasForeignKey(pb => pb.IssuingBranchID).IsRequired(false);

        builder.HasOne(pb => pb.CreatedByUser).WithMany().HasForeignKey(pb => pb.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(pb => pb.UpdatedByUser).WithMany().HasForeignKey(pb => pb.UpdatedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StatusTransitionLogConfiguration : IEntityTypeConfiguration<StatusTransitionLog>
{
    public void Configure(EntityTypeBuilder<StatusTransitionLog> builder)
    {
        builder.HasKey(e => e.TransitionID);
        builder.HasOne(stl => stl.SystemTable).WithMany(st => st.StatusTransitionLogs).HasForeignKey(stl => stl.TableID);
    }
}

public class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.HasKey(e => e.Id);
<<<<<<< HEAD
        builder.HasIndex(ura => new { ura.UserId, ura.RoleId, ura.BranchId, ura.RegionId }).IsUnique().HasFilter("[IsActive] = 1");
=======
        builder.HasIndex(ura => new { ura.UserId, ura.RoleId, ura.BranchId }).IsUnique().HasFilter("[IsActive] = 1");
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        
        builder.HasOne(ura => ura.User).WithMany(u => u.UserRoleAssignments).HasForeignKey(ura => ura.UserId);
        builder.HasOne(ura => ura.Role).WithMany(r => r.UserRoleAssignments).HasForeignKey(ura => ura.RoleId);
        builder.HasOne(ura => ura.Branch).WithMany(b => b.UserRoleAssignments).HasForeignKey(ura => ura.BranchId).IsRequired(false);
        builder.HasOne(ura => ura.Region).WithMany().HasForeignKey(ura => ura.RegionId).IsRequired(false);
    }
}

public class ReportConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.HasKey(e => e.TemplateID);
    }
}

public class SavedReportConfiguration : IEntityTypeConfiguration<SavedReport>
{
    public void Configure(EntityTypeBuilder<SavedReport> builder)
    {
        builder.HasKey(e => e.ReportID);

        builder.Property(e => e.ReportName).HasMaxLength(200).IsRequired();

        builder.HasOne(sr => sr.ReportTemplate).WithMany().HasForeignKey(sr => sr.TemplateID);
    }
}

public class RetireeCategoryConfiguration : IEntityTypeConfiguration<RetireeCategory>
{
    public void Configure(EntityTypeBuilder<RetireeCategory> builder)
    {
        builder.HasKey(e => e.CategoryID);

        builder.Property(e => e.CategoryName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
    }
}

public class ReportConfiguration_Extra : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.Template).IsRequired();
        builder.Property(e => e.QueryTemplate);
    }
}
