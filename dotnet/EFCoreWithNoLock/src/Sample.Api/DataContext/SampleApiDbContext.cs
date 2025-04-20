using Microsoft.EntityFrameworkCore;
using Sample.Api.Entities;

namespace Sample.Api.DataContext
{
    public class SampleApiDbContext(DbContextOptions<SampleApiDbContext> opts) : DbContext(opts)
    {
        public DbSet<Product> Products { get; set; }
    }
}