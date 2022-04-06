using CatalogueService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogueService.Data;

public class CatalogueContext : DbContext
{
    public CatalogueContext(DbContextOptions<CatalogueContext> opt) :base(opt)
    {

    }

    public DbSet<CatalogueItem> CatalogueItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CatalogueItemEntityTypeConfiguration());
    }
}

public class CatalogueContextDesignFactory : IDesignTimeDbContextFactory<CatalogueContext>
{
    public CatalogueContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogueContext>()
            //.UseInMemoryDatabase("InMem");
            .UseSqlServer("Server=.;Initial Catalog=MicroWebShop.Services.CatalogueDb;Integrated Security=true");
        return new CatalogueContext(optionsBuilder.Options);
    }
}