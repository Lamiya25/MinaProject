using Microsoft.EntityFrameworkCore;
using Mina.Entities;

namespace Mina.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Poi> POIs { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Entities.Route> Routes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Poi>(entity =>
            {
                entity.Property(e => e.Properties).HasColumnType("jsonb");
                entity.Property(e => e.Location).HasColumnType("geometry(Point, 4326)");
            });
            modelBuilder.Entity<Building>(entity =>
            {
                entity.Property(e => e.Geometry)
                      .HasColumnType("geometry(Polygon, 4326)");
            });
        }

    }
}