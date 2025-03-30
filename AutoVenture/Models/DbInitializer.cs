using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace YourProjectName.Models
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.Cars.Any()) return;

            var cars = new[]
            {
                new Car { Make = "Toyota", Model = "Corolla", Year = 2020, DailyRentPrice = 50 },
                new Car { Make = "Honda", Model = "Civic", Year = 2019, DailyRentPrice = 45 },
                new Car { Make = "Ford", Model = "Focus", Year = 2021, DailyRentPrice = 55 }
            };

            context.Cars.AddRange(cars);
            context.SaveChanges();
        }
    }
}
