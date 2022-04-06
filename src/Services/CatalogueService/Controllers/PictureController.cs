using System.Net;
using CatalogueService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogueService.Controllers;

[ApiController]
public class PictureController : ControllerBase
{
    private readonly CatalogueContext _catalogueContext;
    private readonly IWebHostEnvironment _env;

    public PictureController(CatalogueContext catalogueContext, IWebHostEnvironment env)
    {
        _catalogueContext = catalogueContext;
        _env = env;
    }

    [HttpGet]
    [Route("api/v1/catalogue/items/{catalogueItemId:int}/pic")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> GetImageAsync(int catalogueItemId)
    {
        if (catalogueItemId <= 0)
        {
            return BadRequest();
        }

        var item = await _catalogueContext.CatalogueItems.SingleOrDefaultAsync(ci => ci.Id == catalogueItemId);

        if (item != null)
        {
            var wb = _env.WebRootPath;
            var path = Path.Combine(wb, item.PictureFileName);
            string pictureFileEx = Path.GetExtension(item.PictureFileName);
            string mimeType = GetImageMime(pictureFileEx);
            var buffer = await System.IO.File.ReadAllBytesAsync(path);

            return File(buffer, mimeType);
        }

        return NotFound();
    }

    private string GetImageMime(string ex)
    {
        string mimeType;

        switch(ex)
        {
            case ".jpg":
                mimeType = "image/jpg";
                break;
            case ".jpeg":
                mimeType = "image/jpeg";
                break;
            case ".png":
                mimeType = "image/png";
                break;
            default:
                mimeType = "";
                break;
        }

        return mimeType;
    }
}