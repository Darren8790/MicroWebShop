using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogueService;
using CatalogueService.Controllers;
using CatalogueService.Data;
using CatalogueService.Model;
using CatalogueService.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CatalogueUnitTests;

public class CatalogueControllerTest
{
    private readonly DbContextOptions<CatalogueContext> _dbOptions;

    public CatalogueControllerTest()
    {  
       _dbOptions = new DbContextOptionsBuilder<CatalogueContext>()
        .UseInMemoryDatabase(databaseName: "InMem")
        .Options;

        using(var dbContext = new CatalogueContext(_dbOptions)) 
        {
            dbContext.AddRange(GetCatalogueTest());
            dbContext.SaveChanges();
        }
    }

    [Fact]
    private List<CatalogueItem> GetCatalogueTest()
    {
        return new List<CatalogueItem>()
        {
            new CatalogueItem()
            {
                Id = 1,
                Name = "AdidasTest",
                PictureFileName = "adidasTestPic.png"
            },
            new CatalogueItem()
            {
                Id = 2,
                Name = "PumaTest",
                PictureFileName = "pumaTestPic.png"
            },
            new CatalogueItem()
            {
                Id = 3,
                Name = "ReebokTest",
                PictureFileName = "ReebokTestPic.png"
            },
            new CatalogueItem()
            {
                Id = 4,
                Name = "NikeTest",
                PictureFileName = "NikeTestPic.png"
            },
            new CatalogueItem()
            {
                Id = 5,
                Name = "NewBalanceTest",
                PictureFileName = "NewBalanceTestPic.png"
            },
        };
    }

    //Todo
    [Fact]
    public async Task GetItemsTest()
    {
        var catalogueContext = new CatalogueContext(_dbOptions);
        var catalogueSettings = new TestCatalogueSettings();

        var pageSize = 3;
        var pageIndex = 1;
        var expectedItemsInPage = 2;
        var expectedTotalItems = 5;
        //var testName = "ReebokTest";
        var testItemId = 1;

        var testController = new CatalogueController(catalogueContext, catalogueSettings);
        //var actionResult = await testController.GetItemsAsync(pageSize, pageIndex);
        var actionResult = await testController.GetItemByIdAsync(testItemId, pageIndex);
        //var actionResultName = await testController.GetItemsWithNameAsync(testName, pageSize, pageIndex);

        // Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogueItem>>>(actionResultName);
        // var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogueItem>>(actionResultName.Value);
        
        Assert.IsType<ActionResult<CatalogueItem>>(actionResult);
        var page = Assert.IsAssignableFrom<ActionResult<CatalogueItem>>(actionResult);

        // Assert.Equal(expectedTotalItems, page.Count);
        // Assert.Equal(pageIndex, page.PageIndex);
        // Assert.Equal(pageSize, page.PageSize);
        // Assert.Equal(expectedItemsInPage, page.Data.Count());
    }
}

public class TestCatalogueSettings : IOptionsSnapshot<CatalogueSettings>
{
    public CatalogueSettings Value => new CatalogueSettings
    {
        PicBaseUrl = "http://image-server.com/"
    };

    public CatalogueSettings Get(string name) => Value;
}