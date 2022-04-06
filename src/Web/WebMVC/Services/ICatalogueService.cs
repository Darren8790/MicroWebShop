using WebMVC.ViewModels;

namespace WebMVC.Services;

public interface ICatalogueService
{
    Task<Catalogue> GetCatalogueItems(int page, int take);
    //Task<Catalogue> GetCatalogueItems();
    //Task<CatalogueItem> GetCatalogueItems();
}