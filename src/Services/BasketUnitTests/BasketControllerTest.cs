using System.Collections.Generic;
using System.Threading.Tasks;
using BasketService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebMVC.Controllers;
using WebMVC.Services;
using WebMVC.ViewModels;
using Xunit;

namespace BasketUnitTests;

public class BasketControllerTest
{
    private readonly Mock<ICatalogueService> _catalogueServiceMock;
    private readonly Mock<IBasketService> _basketServiceMock;
    private readonly Mock<IIdentityParser<AppUser>> _identityParserMock;
    private readonly Mock<HttpContext> _contextMock;

    public BasketControllerTest()
    {
        _catalogueServiceMock = new Mock<ICatalogueService>();
        _basketServiceMock = new Mock<IBasketService>();
        _identityParserMock = new Mock<IIdentityParser<AppUser>>();
        _contextMock = new Mock<HttpContext>();
    }

    [Fact]
    public async Task PostBasketTest()
    {
        var testBuyerId = "1";
        var action = string.Empty;
        var fakeBasket = GetFakeBasket(testBuyerId);
        var fakeQuantities = new Dictionary<string, int>()
        {
            ["fakeProductA"] = 1,
            ["fakeProductB"] = 2
        };

        _basketServiceMock.Setup(x => x.SetQuantities(It.IsAny<AppUser>(), It.IsAny<Dictionary<string, int>>()))
            .Returns(Task.FromResult(fakeBasket));
        _basketServiceMock.Setup(x => x.UpdateBasket(It.IsAny<BasketModel>()))
            .Returns(Task.FromResult(fakeBasket));

        var basketController = new WebMVC.Controllers.BasketController(_basketServiceMock.Object, _catalogueServiceMock.Object, _identityParserMock.Object);
        basketController.ControllerContext.HttpContext = _contextMock.Object;
        var actionResult = await basketController.Index(fakeQuantities, action);

        var viewResult = Assert.IsType<ViewResult>(actionResult);
    }

    [Fact]
    public async Task PostBasketCheckoutTest()
    {
        var testBuyerId = "1";
        var action = "[ Checkout ]";
        var fakeBasket = GetFakeBasket(testBuyerId);
        var fakeQuantities = new Dictionary<string, int>()
        {
            ["fakeProductA"] = 1,
            ["fakeProductB"] = 2
        };

        _basketServiceMock.Setup(x => x.SetQuantities(It.IsAny<AppUser>(), It.IsAny<Dictionary<string, int>>()))
            .Returns(Task.FromResult(fakeBasket));
        _basketServiceMock.Setup(x => x.UpdateBasket(It.IsAny<BasketModel>()))
            .Returns(Task.FromResult(fakeBasket));
        
        var testController = new WebMVC.Controllers.BasketController(_basketServiceMock.Object, _catalogueServiceMock.Object, _identityParserMock.Object);
        testController.ControllerContext.HttpContext = _contextMock.Object;
        var actionResult = await testController.Index(fakeQuantities, action);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal("Order", redirectToActionResult.ControllerName);
        Assert.Equal("Create", redirectToActionResult.ActionName);
    }

    [Fact]
    public async Task AddToBasketTest()
    {
        var fakeCatalogueItem = GetFakeCatalogueItem();
        _basketServiceMock.Setup(x => x.AddItemToBasket(It.IsAny<AppUser>(), It.IsAny<Int32>()))
            .Returns(Task.FromResult(1));
        
        var testController = new WebMVC.Controllers.BasketController(_basketServiceMock.Object, _catalogueServiceMock.Object, _identityParserMock.Object);
        testController.ControllerContext.HttpContext = _contextMock.Object;
        var actionResult = await testController.AddToBasket(fakeCatalogueItem);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal("Catalogue", redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

    private Basket GetFakeBasket(string buyerId)
    {
        return new Basket()
        {
            BuyerId = buyerId
        };
    }

    private CatalogueItem GetFakeCatalogueItem()
    {
        return new CatalogueItem()
        {
            Id = 1,
            Name = "fakeAdidas",
            Price = 60
        };
    }
}