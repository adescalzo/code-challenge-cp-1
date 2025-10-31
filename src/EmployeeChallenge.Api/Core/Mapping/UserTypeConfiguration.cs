using EmployeeChallenge.Api.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeChallenge.Api.Core.Mapping;

internal class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.Username).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Password).IsRequired();
    }
}
