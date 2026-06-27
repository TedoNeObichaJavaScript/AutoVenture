using Microsoft.EntityFrameworkCore;

namespace AutoVenture.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Rental> Rentals => Set<Rental>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Car>(car =>
            {
                // Fixed currency precision — avoids silent truncation of prices.
                car.Property(c => c.DailyRentPrice).HasPrecision(18, 2);
                car.Property(c => c.RowVersion).IsRowVersion();
            });

            modelBuilder.Entity<Rental>(rental =>
            {
                // Covering index for the availability/overlap query used by the
                // booking service — turns the conflict scan into an index seek.
                rental.HasIndex(r => new { r.CarId, r.StartDate, r.EndDate })
                      .HasDatabaseName("IX_Rentals_Car_Window");

                rental.HasOne(r => r.Car)
                      .WithMany(c => c.Rentals)
                      .HasForeignKey(r => r.CarId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
