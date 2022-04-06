using System.Data.SqlClient;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using CatalogueService.Extensions;
using CatalogueService.Model;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CatalogueService.Data;

public class CatalogueContextSeed
{
    public async Task SeedAsync(CatalogueContext context, IWebHostEnvironment env, IOptions<CatalogueSettings> settings, ILogger<CatalogueContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(CatalogueContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var useCustomizationData = settings.Value.UseCustomizationData;
            var contentRootPath = env.ContentRootPath;
            var picturePath = env.WebRootPath;

            if (!context.CatalogueItems.Any())
            {
                await context.CatalogueItems.AddRangeAsync(useCustomizationData
                    ? GetCatalogueItemsFromFile(contentRootPath, context, logger)
                    : GetPreconfiguredItems());

                await context.SaveChangesAsync();

                GetCatalogueItemPictures(contentRootPath, picturePath);
            }
        });
    }

    private IEnumerable<CatalogueItem> GetCatalogueItemsFromFile(string contentRootPath, CatalogueContext context, ILogger<CatalogueContextSeed> logger)
    {
        string csvFileCatalogueItems = Path.Combine(contentRootPath, "Setup", "CatalogueItems.csv");

        if (!File.Exists(csvFileCatalogueItems))
        {
            return GetPreconfiguredItems();
        }

        string[] csvheaders;
        try
        {
            string[] requiredHeaders = { "description", "name", "price", "picturefilename" };
            string[] optionalheaders = { "availablestock" };
            csvheaders = GetHeaders(csvFileCatalogueItems, requiredHeaders, optionalheaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPreconfiguredItems();
        }

        return File.ReadAllLines(csvFileCatalogueItems)
                    .Skip(1) // skip header row
                    .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                    .SelectTry(column => CreateCatalogueItem(column, csvheaders))
                    .OnCaughtException(ex => { logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message); return null; })
                    .Where(x => x != null);
    }

    private CatalogueItem CreateCatalogueItem(string[] column, string[] headers)
    {
        if (column.Count() != headers.Count())
        {
            throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
        }

        string priceString = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
        if (!Decimal.TryParse(priceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Decimal price))
        {
            throw new Exception($"price={priceString}is not a valid decimal number");
        }

        var catalogueItem = new CatalogueItem()
        {
            Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
            Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
            Price = price,
            PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
        };

        int availableStockIndex = Array.IndexOf(headers, "availablestock");
        if (availableStockIndex != -1)
        {
            string availableStockString = column[availableStockIndex].Trim('"').Trim();
            if (!String.IsNullOrEmpty(availableStockString))
            {
                if (int.TryParse(availableStockString, out int availableStock))
                {
                    catalogueItem.AvailableStock = availableStock;
                }
                else
                {
                    throw new Exception($"availableStock={availableStockString} is not a valid integer");
                }
            }
        }
        return catalogueItem;
    }

    private IEnumerable<CatalogueItem> GetPreconfiguredItems()
    {
        return new List<CatalogueItem>()
        {
            new CatalogueItem {Name="Puma Shoes", Description="Trainers", Price=50.00M, AvailableStock=100, PictureFileName="Shoe1.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Puma Shoes", Description="Trainers", Price=50.00M, AvailableStock=100, PictureFileName="Shoe1.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
            new CatalogueItem {Name="Adidas Shoes", Description="Trainers", Price=60.00M, AvailableStock=100, PictureFileName="Shoe2.jpg"},
        };
    }

    private string[] GetHeaders(string csvfile, string[] requiredHeaders, string[] optionalHeaders = null)
    {
        string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

        if (csvheaders.Count() < requiredHeaders.Count())
        {
            throw new Exception($"requiredHeader count '{ requiredHeaders.Count()}' is bigger then csv header count '{csvheaders.Count()}' ");
        }

        if (optionalHeaders != null)
        {
            if (csvheaders.Count() > (requiredHeaders.Count() + optionalHeaders.Count()))
            {
                throw new Exception($"csv header count '{csvheaders.Count()}'  is larger then required '{requiredHeaders.Count()}' and optional '{optionalHeaders.Count()}' headers count");
            }
        }

        foreach (var requiredHeader in requiredHeaders)
        {
            if (!csvheaders.Contains(requiredHeader))
            {
                throw new Exception($"does not contain required header '{requiredHeader}'");
            }
        }

        return csvheaders;
    }

    private void GetCatalogueItemPictures(string contentRootPath, string picturePath)
    {
        if (picturePath != null)
        {
            DirectoryInfo directory = new DirectoryInfo(picturePath);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            string zipFileCatalogueItemPictures = Path.Combine(contentRootPath, "Setup", "CatalogueItems.zip");
            ZipFile.ExtractToDirectory(zipFileCatalogueItemPictures, picturePath);
        }
    }

    private AsyncRetryPolicy CreatePolicy(ILogger<CatalogueContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", prefix, exception.GetType().Name, exception.Message, retry, retries);
                }
            );
    }
}