using System;
using System.Linq;

namespace AutoVenture.Services
{
    /// <summary>Validated input for creating a rental from the public site.</summary>
    public class BookingRequest
    {
        public int CarId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Egn { get; set; } = string.Empty;
        public string PickupSite { get; set; } = string.Empty;

        public string CreditCard { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty; // MM/YY
        public string Ccv { get; set; } = string.Empty;

        public string FullName =>
            string.Join(' ', new[] { FirstName, MiddleName, LastName }
                .Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
