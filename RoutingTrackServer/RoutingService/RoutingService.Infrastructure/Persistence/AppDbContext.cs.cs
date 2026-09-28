using Microsoft.EntityFrameworkCore;
using RoutingService.Domain.Entities;

namespace RoutingService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RouteLog> RouteLogs => Set<RouteLog>();

    public DbSet<TrackedRoute> TrackedRoutes => Set<TrackedRoute>();

    public DbSet<RouteSnapshot> RouteSnapshots => Set<RouteSnapshot>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        
        modelBuilder.Entity<RouteLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FromAddress)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ToAddress)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Provider)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.TransportMode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Error)
                .HasMaxLength(2000);

            entity.HasIndex(x => new
            {
                x.RequestedAt,
                x.Provider,
                x.TransportMode
            });
        });



        modelBuilder.Entity<TrackedRoute>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FromAddress)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ToAddress)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasMany(x => x.Snapshots)
                .WithOne(x => x.TrackedRoute!)
                .HasForeignKey(x => x.TrackedRouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        
        
        modelBuilder.Entity<RouteSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Provider)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.TransportMode)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.TrackedRouteId,
                x.CapturedAt
            });
        });


    }


}