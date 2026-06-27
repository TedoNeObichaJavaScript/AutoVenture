using System;

namespace AutoVenture.Validation
{
    /// <summary>
    /// Parses and validates a Bulgarian EGN (ЕГН) — 10 digits encoding the
    /// holder's date of birth plus a control checksum. The month field encodes
    /// the century: 1-12 → 1900s, 21-32 → 1800s, 41-52 → 2000s.
    /// </summary>
    public static class BulgarianEgn
    {
        private static readonly int[] Weights = { 2, 4, 8, 5, 10, 9, 7, 3, 6 };

        public static bool TryParse(string? egn, out DateTime birthDate)
        {
            birthDate = default;
            if (string.IsNullOrWhiteSpace(egn) || egn.Length != 10)
                return false;

            Span<int> d = stackalloc int[10];
            for (int i = 0; i < 10; i++)
            {
                if (!char.IsDigit(egn[i])) return false;
                d[i] = egn[i] - '0';
            }

            int year = d[0] * 10 + d[1];
            int month = d[2] * 10 + d[3];
            int day = d[4] * 10 + d[5];

            int century;
            if (month >= 1 && month <= 12) { century = 1900; }
            else if (month >= 21 && month <= 32) { century = 1800; month -= 20; }
            else if (month >= 41 && month <= 52) { century = 2000; month -= 40; }
            else return false;

            int fullYear = century + year;
            if (day < 1 || day > DateTime.DaysInMonth(fullYear, month))
                return false;

            // Control digit check.
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += d[i] * Weights[i];
            int control = sum % 11;
            if (control == 10) control = 0;
            if (control != d[9]) return false;

            birthDate = new DateTime(fullYear, month, day);
            return true;
        }

        public static int AgeOn(DateTime birthDate, DateTime asOf)
        {
            int age = asOf.Year - birthDate.Year;
            if (birthDate.Date > asOf.Date.AddYears(-age)) age--;
            return age;
        }
    }
}
