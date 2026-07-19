using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Users;

namespace Users.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Id).ValueGeneratedNever();
        
        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Login).IsUnique();
    }
}