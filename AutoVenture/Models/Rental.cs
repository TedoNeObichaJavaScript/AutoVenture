using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        [ForeignKey("Car")]
        public int CarId { get; set; }

        public virtual Car Car { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // This property is not mapped since it's calculated.
        [NotMapped]
        public decimal TotalPrice
        {
            get
            {
                int rentalDays = (EndDate - StartDate).Days + 1;
                return rentalDays * Car?.DailyRentPrice ?? 0;
            }
        }
    }
}
