namespace WebMVC.ViewModels;

public record Catalogue
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int Count { get; init; }
    public List<CatalogueItem> Data { get; init; }
}