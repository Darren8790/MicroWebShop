using System.IO.Compression;
using CatalogueService.Model;
using Microsoft.Extensions.Options;

namespace CatalogueService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        using(var serviceScope = app.ApplicationServices.CreateScope())
        {
            SeedData(serviceScope.ServiceProvider.GetService<CatalogueContext>());
        }
    }

    private static void SeedData(CatalogueContext context)
    {
        if(!context.CatalogueItems.Any())
        {
            Console.WriteLine("--> Seeding Data...");

            context.CatalogueItems.AddRange(
                new CatalogueItem() {Name="Puma Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe1.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Puma Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe1.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"},
                new CatalogueItem() {Name="Adidas Shoes", Description="Trainers", Price=50.00M, AvailableStock=1, PictureFileName="Shoe2.jpg"}
            );

            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("--> We already have data");
        }
    }

    private static void SeedImages(string contentRootPath, string picturePath)
    {
        if(picturePath != null)
        {
            DirectoryInfo directory = new DirectoryInfo(picturePath);
            foreach(FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            string zipFileCatalogueItems = Path.Combine(contentRootPath, "Setup", "CatalogueItems.zip");
            ZipFile.ExtractToDirectory(zipFileCatalogueItems, picturePath);
        }
    }
}