using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensionMS.Core.Entities.OperationalBudget;

namespace PensionMS.Infrastructure.Data.Configurations.Budget;

public class OperationalExpenseConfiguration : IEntityTypeConfiguration<OperationalExpense>
{
    public void Configure(EntityTypeBuilder<OperationalExpense> builder)
    {
        builder.ToTable("OperationalExpenses");
        builder.HasKey(e => e.ExpenseID);
        
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.NetAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedByUserID);
    }
}
