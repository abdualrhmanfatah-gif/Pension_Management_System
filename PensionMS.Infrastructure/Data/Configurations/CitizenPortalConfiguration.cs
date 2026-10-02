using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Data.Configurations;

public class CitizenAccountConfiguration : IEntityTypeConfiguration<CitizenAccount>
{
    public void Configure(EntityTypeBuilder<CitizenAccount> builder)
    {
        builder.HasKey(e => e.CitizenAccountID);

        builder.HasIndex(e => e.RetireeID).IsUnique();
        builder.HasIndex(e => e.NationalID).IsUnique();
        
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.NationalID).IsRequired().HasMaxLength(20);
        builder.Property(e => e.RefreshToken).HasMaxLength(500);

        builder.HasOne(a => a.Retiree)
               .WithOne()
               .HasForeignKey<CitizenAccount>(a => a.RetireeID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.DeletedByUser).WithMany().HasForeignKey(c => c.DeletedByUserID);
    }
}

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.HasKey(e => e.OtpVerificationID);
        
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.OtpHash).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Purpose).IsRequired().HasMaxLength(50);

        builder.HasOne(o => o.CitizenAccount)
               .WithMany()
               .HasForeignKey(o => o.CitizenAccountID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.HasKey(e => e.RequestID);

        builder.Property(e => e.RequestType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        
        builder.HasOne(r => r.CitizenAccount)
               .WithMany()
               .HasForeignKey(r => r.CitizenAccountID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RetirementFile)
               .WithMany()
               .HasForeignKey(r => r.FileID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ProcessedByUser)
               .WithMany()
               .HasForeignKey(r => r.ProcessedByUserID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasKey(e => e.SupportTicketID);

        builder.HasIndex(e => e.TicketNumber).IsUnique();
        
        builder.Property(e => e.TicketNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Priority).IsRequired().HasMaxLength(20);

        builder.HasOne(t => t.CitizenAccount)
               .WithMany()
               .HasForeignKey(t => t.CitizenAccountID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser)
               .WithMany()
               .HasForeignKey(t => t.AssignedToUserID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DeletedByUser).WithMany().HasForeignKey(t => t.DeletedByUserID);
    }
}

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.HasKey(e => e.TicketMessageID);

        builder.Property(e => e.SenderType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Content).HasMaxLength(4000).IsRequired();

        builder.HasOne(m => m.Ticket)
               .WithMany(t => t.Messages)
               .HasForeignKey(m => m.TicketID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.DeletedByUser).WithMany().HasForeignKey(m => m.DeletedByUserID);
    }
}

public class CitizenNotificationConfiguration : IEntityTypeConfiguration<CitizenNotification>
{
    public void Configure(EntityTypeBuilder<CitizenNotification> builder)
    {
        builder.HasKey(e => e.NotificationID);
        
        builder.HasOne(n => n.Retiree)
               .WithMany()
               .HasForeignKey(n => n.RetireeID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
