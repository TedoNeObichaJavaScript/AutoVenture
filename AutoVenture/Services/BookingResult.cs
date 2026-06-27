namespace AutoVenture.Services
{
    public enum BookingOutcome
    {
        Success,
        CarNotFound,
        NotAvailable,
        Conflict
    }

    public record BookingResult(BookingOutcome Outcome, int? RentalId = null, decimal Total = 0m)
    {
        public bool Succeeded => Outcome == BookingOutcome.Success;

        public static BookingResult Success(int rentalId, decimal total) =>
            new(BookingOutcome.Success, rentalId, total);
    }
}
