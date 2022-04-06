namespace CatalogueService.Dtos;

public class CatalogueReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Decimal Price { get; set; }
    public int AvailableStock { get; set; }
    public string PictureName { get; set; }
    public string PictureUri { get; set; }
}