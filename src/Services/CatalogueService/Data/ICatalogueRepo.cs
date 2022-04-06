using CatalogueService.Model;

namespace CatalogueService.Data;

public interface ICatalogueRepo
{
    bool SaveChanges();

    IEnumerable<CatalogueItem> GetAllCatalogueItems();
    CatalogueItem GetCatalogueItemById(int id);
    void CreateCatalogueItem(CatalogueItem item);
}