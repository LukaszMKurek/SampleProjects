using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Database
{
    public sealed class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
        {
            ExampleTable = Set<ExampleTable>();
        }

        public DbSet<ExampleTable> ExampleTable { get; }
    }

    public sealed class ExampleTable
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
