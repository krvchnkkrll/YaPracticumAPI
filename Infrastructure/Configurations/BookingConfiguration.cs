using Domain.Entities.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.Id).ValueGeneratedNever();
        
        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.EventId)
            .IsRequired();

    }
}