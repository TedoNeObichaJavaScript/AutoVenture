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
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public decimal TotalPrice
        {
            get
            {
                if (EndDate.HasValue && Car != null)
                {
                    int rentalDays = (EndDate.Value - StartDate).Days + 1;
                    return rentalDays * Car.DailyRentPrice;
                }
                return 0;
            }
        }
    }
}
