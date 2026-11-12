using Microsoft.EntityFrameworkCore;
using MeetingRoomManagement.Api.Domain.Entities;

namespace MeetingRoomManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(150);
            entity.Property(r => r.Capacity).IsRequired();
            entity.Property(r => r.Floor).IsRequired();

            entity.HasMany(r => r.Bookings)
                  .WithOne(b => b.Room)
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.BookedBy).IsRequired().HasMaxLength(150);
            entity.Property(b => b.StartTime).IsRequired();
            entity.Property(b => b.EndTime).IsRequired();
            entity.Property(b => b.CreatedAt).IsRequired();

            entity.HasIndex(b => new { b.RoomId, b.StartTime, b.EndTime });
        });
    }
}
