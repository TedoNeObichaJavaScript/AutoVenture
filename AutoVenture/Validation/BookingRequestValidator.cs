using System;
using System.Linq;
using AutoVenture.Services;
using FluentValidation;

namespace AutoVenture.Validation
{
    public class BookingRequestValidator : AbstractValidator<BookingRequest>
    {
        public const int MinDriverAge = 21;
        public const int MaxRentalDays = 90;

        public BookingRequestValidator()
        {
            RuleFor(x => x.CarId).GreaterThan(0);

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(60);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(60);
            RuleFor(x => x.MiddleName).MaximumLength(60);
            RuleFor(x => x.PickupSite).NotEmpty().MaximumLength(120);

            // Date window: start today or later, end strictly after start,
            // capped at MaxRentalDays.
            RuleFor(x => x.StartDate)
                .Must(d => d.Date >= DateTime.UtcNow.Date)
                .WithMessage("Start date cannot be in the past.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after the start date.");

            RuleFor(x => x)
                .Must(x => (x.EndDate.Date - x.StartDate.Date).Days <= MaxRentalDays)
                .WithMessage($"Rental period cannot exceed {MaxRentalDays} days.")
                .WithName(nameof(BookingRequest.EndDate));

            // Age restriction derived from the EGN birth date.
            RuleFor(x => x.Egn)
                .Must(BeValidEgnWithSufficientAge)
                .WithMessage($"Driver must be a valid ID holder aged {MinDriverAge}+.");

            // Payment: Luhn-valid card, MM/YY expiry not in the past, 3-4 digit CCV.
            RuleFor(x => x.CreditCard)
                .Must(BeLuhnValid)
                .WithMessage("Invalid credit card number.");

            RuleFor(x => x.ExpirationDate)
                .Must(NotBeExpired)
                .WithMessage("Card expiry must be a valid future MM/YY.");

            RuleFor(x => x.Ccv)
                .Matches(@"^\d{3,4}$")
                .WithMessage("CCV must be 3 or 4 digits.");
        }

        private static bool BeValidEgnWithSufficientAge(string egn)
        {
            if (!BulgarianEgn.TryParse(egn, out var dob)) return false;
            return BulgarianEgn.AgeOn(dob, DateTime.UtcNow) >= MinDriverAge;
        }

        private static bool BeLuhnValid(string card)
        {
            if (string.IsNullOrWhiteSpace(card)) return false;
            var digits = card.Where(char.IsDigit).ToArray();
            if (digits.Length is < 13 or > 19) return false;

            int sum = 0;
            bool dbl = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (dbl) { n *= 2; if (n > 9) n -= 9; }
                sum += n;
                dbl = !dbl;
            }
            return sum % 10 == 0;
        }

        private static bool NotBeExpired(string mmYy)
        {
            var parts = mmYy?.Split('/');
            if (parts is not { Length: 2 }) return false;
            if (!int.TryParse(parts[0], out int month) || month is < 1 or > 12) return false;
            if (!int.TryParse(parts[1], out int yy)) return false;

            int year = 2000 + yy;
            // Card is valid through the last day of its expiry month.
            var expiryEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return expiryEnd >= DateTime.UtcNow.Date;
        }
    }
}
