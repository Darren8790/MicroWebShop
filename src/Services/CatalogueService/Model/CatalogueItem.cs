using System.ComponentModel.DataAnnotations;

namespace CatalogueService.Model;

public class CatalogueItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int AvailableStock { get; set; }
    public string PictureFileName { get; set; }
    public string PictureUri { get; set; }
    public CatalogueItem() {}
}
