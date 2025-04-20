using Microsoft.EntityFrameworkCore;
using Sample.Api.Entities;

namespace Sample.Api.DataContext
{
    public static class SeedData
    {
        public static async Task EnsurePopulatedAsync(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();

            var sampleApiDbContext = scope.ServiceProvider.GetRequiredService<SampleApiDbContext>();

            // Ensure the database is created
            if ((await sampleApiDbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await sampleApiDbContext.Database.MigrateAsync();
            }

            // Insert sample data if the database is empty
            if (await sampleApiDbContext.Products.AsNoTracking().AnyAsync())
            {
                return;
            }

            Product[] sampleProducts =
            [
                new Product
                    {
                        Name = "Sample Product 1",
                        Price = 10.99m,
                        Description = "This is a sample product.",
                        Stock = 100
                    },
                    new Product{
                        Name = "Sample Product 2",
                        Price = 20.99m,
                        Description = "This is another sample product.",
                        Stock = 50
                    },
                    new Product
                    {
                        Name = "Sample Product 3",
                        Price = 30.99m,
                        Description = "This is yet another sample product.",
                        Stock = 25
                    }
            ];

            sampleApiDbContext.Products.AddRange(sampleProducts);

            await sampleApiDbContext.SaveChangesAsync();

            sampleApiDbContext.ChangeTracker.Clear();
        }
    }
}