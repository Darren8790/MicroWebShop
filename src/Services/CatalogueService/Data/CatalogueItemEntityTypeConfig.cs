using CatalogueService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogueService.Data;

public class CatalogueItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogueItem>
{
    public void Configure(EntityTypeBuilder<CatalogueItem> builder)
    {
        builder.ToTable("Catalogue");

        builder.Property(ci => ci.Id)
            .UseHiLo("catalogue_hilo")
            .IsRequired();

        builder.Property(ci => ci.Name)
            .IsRequired(true)
            .HasMaxLength(50);
        
        builder.Property(ci => ci.Price)
            .IsRequired(true);
        
        builder.Property(ci => ci.PictureFileName)
            .IsRequired(false);
        
        builder.Ignore(ci => ci.PictureUri);
    }
}