namespace WebMVC.ViewModels;

public record CatalogueItem
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
    //public int AvailableStock { get; set; }
    //public string PictureName { get; init; }
    public string PictureUri { get; init; }
}