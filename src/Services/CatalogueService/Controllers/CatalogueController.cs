using System.Net;
using AutoMapper;
using CatalogueService.Data;
using CatalogueService.Dtos;
using CatalogueService.Model;
using CatalogueService.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatalogueService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CatalogueController : ControllerBase
{
    private readonly CatalogueContext _catalogueContext;
    private readonly CatalogueSettings _settings;

    //private readonly ICatalogueRepo _repository;
    //private readonly IMapper _mapper;

    public CatalogueController(CatalogueContext catalogueContext, IOptionsSnapshot<CatalogueSettings> settings)
    {
        _catalogueContext = catalogueContext;
        _settings = settings.Value;
        //_repository = repository;
        //_mapper = mapper;
        catalogueContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // Get api/v1/[controller]/items[?pageSize=3&pageIndex=10]   Get all items and page info
    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogueItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(IEnumerable<CatalogueItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string ids = null)
    {
        Console.WriteLine("--> Catalogue service controller hit");
        if (!string.IsNullOrEmpty(ids))
        {
            var catalogueItems = await GetItemsByIdsAsync(ids);

            if (!catalogueItems.Any())
            {
                return BadRequest("ids invaild");
            }
            return Ok(catalogueItems);
        }

        var totalItems = await _catalogueContext.CatalogueItems.LongCountAsync();
        var itemsOnPage = await _catalogueContext.CatalogueItems
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();
        
        //itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        var model = new PaginatedItemsViewModel<CatalogueItem>(pageIndex, pageSize, totalItems, itemsOnPage);
        return Ok(model);
    }

    private async Task<List<CatalogueItem>> GetItemsByIdsAsync(string ids)
    {
        Console.WriteLine("--> GetItemsByIdsAsync hit");
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

        if (!numIds.All(nid => nid.Ok))
        {
            return new List<CatalogueItem>();
        }

        var idsToSelect = numIds.Select(id => id.Value);
        var catalogueItems = await _catalogueContext.CatalogueItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();
        //catalogueItems = ChangeUriPlaceholder(catalogueItems);

        return catalogueItems;
    }

    // Get api/v1/[controller]/items/id   Get a single item by id
    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(CatalogueItem), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CatalogueItem>> GetItemByIdAsync(int id, int pageIndex)
    {
        Console.WriteLine("--> GetItemByIdAsync hit");
        if (id <= 0)
        {
            return BadRequest();
        }

        // product images
        var item = await _catalogueContext.CatalogueItems.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item != null)
        {
            return item;
        }

        return NotFound();
    }

    // Get api/v1/[controller]/items/withname/name[?pageSize=3&pageIndex=10]   Get item by name and page info
    [HttpGet]
    [Route("items/withname/{name:minlength(1)}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogueItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogueItem>>> GetItemsWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var totalItems = await _catalogueContext.CatalogueItems
            .Where(c => c.Name.StartsWith(name))
            .LongCountAsync();
        
        var itemsOnPage = await _catalogueContext.CatalogueItems
            .Where(c => c.Name.StartsWith(name))
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();
        
        //itemsOnPage = ChangeUriPlaceholer(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogueItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    // Post api/v1/[controller]/items   Create a new item
    [HttpPost]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreateItemAsync([FromBody] CatalogueItem catalogueItem)
    {
        var item = new CatalogueItem
        {
            Name = catalogueItem.Name,
            Description = catalogueItem.Description,
            Price = catalogueItem.Price,
            PictureFileName = catalogueItem.PictureFileName
        };

        _catalogueContext.CatalogueItems.Add(item);
        await _catalogueContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItemByIdAsync), new { id = item.Id}, null);
    }

    // Put api/v1/[controller]/items   Update item
    [HttpPut]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateItemAsync([FromBody] CatalogueItem itemToUpdate)
    {
        var catalogueItem = await _catalogueContext.CatalogueItems.SingleOrDefaultAsync(i => i.Id == itemToUpdate.Id);

        if(catalogueItem == null)
        {
            return NotFound();
        }

        var oldItemPrice = catalogueItem.Price;
        var newItemPriceEvent = oldItemPrice != itemToUpdate.Price;

        catalogueItem = itemToUpdate;
        _catalogueContext.CatalogueItems.Update(catalogueItem);

        if(newItemPriceEvent)
        {
            // update event
        }
        else
        {
            await _catalogueContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetItemByIdAsync), new {id = itemToUpdate.Id}, null);
    }

    // Delete api/v1/[controller]/id   Delete an item
    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteItemByIdAsync(int id)
    {
        var item = _catalogueContext.CatalogueItems.SingleOrDefault(ci => ci.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        _catalogueContext.CatalogueItems.Remove(item);
        await _catalogueContext.SaveChangesAsync();

        return NoContent();
    }

    // [HttpGet]
    // [Route("items", Name="GetAllCatalogueItems")]
    // public ActionResult<IEnumerable<CatalogueReadDto>> GetAllCatalogueItems()
    // {
    //     Console.WriteLine("--> Getting items...");

    //     var catalogueItem = _repository.GetAllCatalogueItems();

    //     return Ok(_mapper.Map<IEnumerable<CatalogueReadDto>>(catalogueItem));
    // }

    // [HttpGet]
    // [Route("items/{id}", Name="GetCatalogueItemById")]
    // public ActionResult<CatalogueReadDto> GetCatalogueItemById(int id)
    // {
    //     Console.WriteLine("test 2");
    //     var catalogueItem = _repository.GetCatalogueItemById(id);
    //     if (catalogueItem != null)
    //     {
    //         return Ok(_mapper.Map<CatalogueReadDto>(catalogueItem));
    //     }

    //     return NotFound();
    // }

    // [HttpPost]
    // [Route("items")]
    // public ActionResult<CatalogueReadDto> CreateCatalogueItem(CatalogueCreateDto catalogueCreateDto)
    // {
    //     var catalogueModel = _mapper.Map<CatalogueItem>(catalogueCreateDto);
    //     _repository.CreateCatalogueItem(catalogueModel);
    //     _repository.SaveChanges();

    //     var catalogueReadDto = _mapper.Map<CatalogueReadDto>(catalogueModel);

    //     return CreatedAtRoute(nameof(GetCatalogueItemById), new {Id = catalogueReadDto.Id}, catalogueReadDto);
    // }
}