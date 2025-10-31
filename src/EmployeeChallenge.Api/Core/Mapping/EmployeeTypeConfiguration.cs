using EmployeeChallenge.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeChallenge.Api.Core.Mapping;

internal class EmployeeTypeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(100);

        // Self-referencing relationship
        builder.HasOne(e => e.Supervisor)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
