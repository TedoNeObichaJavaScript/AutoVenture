using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoVenture.Models
{
    public class Car
    {
        public int CarId { get; set; }

        [Required]
        [StringLength(60)]
        public required string Make { get; set; }

        [Required]
        [StringLength(60)]
        public required string Model { get; set; }

        [Range(1950, 2100)]
        public int Year { get; set; }

        [DataType(DataType.Currency)]
        [Range(0.01, 100000)]
        public decimal DailyRentPrice { get; set; }

        public string? ImageUrl { get; set; }

        // Optimistic-concurrency token. With SQL Server, byte[] + [Timestamp]
        // maps to a rowversion column the database bumps on every UPDATE,
        // so concurrent edits to the same car are caught at SaveChanges.
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation property: a car can have multiple rentals.
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
