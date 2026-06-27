using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoVenture.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        [ForeignKey(nameof(Car))]
        public int CarId { get; set; }

        // EF-populated navigation; null! suppresses the nullable warning
        // without making the relationship optional.
        public virtual Car Car { get; set; } = null!;

        [Required]
        [StringLength(120)]
        public required string CustomerName { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public int RentalDays =>
            EndDate.HasValue ? (EndDate.Value.Date - StartDate.Date).Days + 1 : 0;

        [NotMapped]
        public decimal TotalPrice =>
            EndDate.HasValue && Car is not null ? RentalDays * Car.DailyRentPrice : 0m;
    }
}
