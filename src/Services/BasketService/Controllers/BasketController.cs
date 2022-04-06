using System.Net;
using System.Security.Claims;
using BasketService.Repository;
using BasketService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;

namespace BasketService.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class BasketController : ControllerBase
{
    private readonly IBasketRepo _basketRepo;
    //private readonly IDatabase _database;

    private readonly IUserIdentity _userIdentity;

    public BasketController(IBasketRepo basketRepo, IUserIdentity userIdentity) //IDatabase database)
    {
        _basketRepo = basketRepo;
        //_database = database;
        _userIdentity = userIdentity;
        Console.WriteLine("--> BasketController");
    }

    //[AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> GetBasketById(string id)
    {
        var basket = await _basketRepo.GetBasket(id);
        return Ok(basket ?? new CustomerBasket(id));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> UpdateBasket([FromBody] CustomerBasket value)
    {
        return Ok(await _basketRepo.UpdateBasket(value));
    }

    [HttpPost]
    [Route("checkout")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> Checkout([FromBody] BasketCheckout basketCheckout, [FromHeader(Name = "x-requestid")] string requestId)
    {
        var userId = _userIdentity.GetUserIdentity();

        basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
        guid : basketCheckout.RequestId;
        
        var basket = await _basketRepo.GetBasket(userId);
        if(basket == null)
        {
            return BadRequest();
        }

        // var userName = this.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Name).Value;
        // var eventMsg = new UserCheckoutAcceptedIntegrationEvent(userId, userName, basketCheckout.Country, basketCheckout.City, basketCheckout.Street
        //     basketCheckout.PostCode, basketCheckout.Buyer, basketCheckout.RequestId, basket);
        
        // try
        // {
        //     _eventBus.Publish(eventMsg);
        // }
        // catch(Exception ex)
        // {
        //     throw;
        // }

        return Accepted();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task DeleteBasketById(string id)
    {
        await _basketRepo.DeleteBasket(id);
    }

}