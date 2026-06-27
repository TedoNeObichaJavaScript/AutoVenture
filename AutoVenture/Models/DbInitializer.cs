using System.Linq;

namespace AutoVenture.Models
{
    public static class DbInitializer
    {
        // Schema creation is owned by Database.Migrate() in Program.cs.
        // This seeder only inserts reference data and is idempotent.
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Cars.Any()) return;

            var cars = new[]
            {
                new Car { Make = "Mazda",  Model = "MX-5",     Year = 2023, DailyRentPrice = 60m, ImageUrl = "/carsphoto/mx5.png" },
                new Car { Make = "Honda",  Model = "CR-V",     Year = 2022, DailyRentPrice = 45m, ImageUrl = "/carsphoto/crv.png" },
                new Car { Make = "Toyota", Model = "Corolla",  Year = 2021, DailyRentPrice = 30m, ImageUrl = "/carsphoto/corolla.png" },
                new Car { Make = "BMW",    Model = "3 Series", Year = 2023, DailyRentPrice = 70m, ImageUrl = "/carsphoto/bmw.png" },
                new Car { Make = "Audi",   Model = "A4",       Year = 2022, DailyRentPrice = 65m, ImageUrl = "/carsphoto/audi.png" },
                new Car { Make = "Ford",   Model = "Focus",    Year = 2021, DailyRentPrice = 28m, ImageUrl = "/carsphoto/focus.png" },
                new Car { Make = "Jeep",   Model = "Wrangler", Year = 2023, DailyRentPrice = 75m, ImageUrl = "/carsphoto/jeep.png" }
            };

            context.Cars.AddRange(cars);
            context.SaveChanges();
        }
    }
}
