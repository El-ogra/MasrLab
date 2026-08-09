using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.OwnsOne(e => e.Period, p =>
        {
            p.Property(pr => pr.Start).HasColumnName("PeriodStart").IsRequired();
            p.Property(pr => pr.End).HasColumnName("PeriodEnd").IsRequired();
        });

        builder.Property(e => e.TotalIncome).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.TotalDiscount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.NetActivityAfterCommission).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.AccountType).IsRequired();

        builder.HasIndex(e => e.DoctorId);
        builder.HasIndex(e => e.ReferralEntityId);
        builder.HasIndex(e => e.IsDeleted);
    }
}
