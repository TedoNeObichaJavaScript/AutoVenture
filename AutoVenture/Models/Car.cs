using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Models
{
    public class Car
    {
        public int CarId { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        public int Year { get; set; }

        [DataType(DataType.Currency)]
        public decimal DailyRentPrice { get; set; }

        // Navigation property: A car can have multiple rentals.
        public virtual ICollection<Rental> Rentals { get; set; }
    
        public string? ImageUrl { get; set; }
     }
 }

