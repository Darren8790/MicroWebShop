using CatalogueService.Model;

namespace CatalogueService.Data;

public class CatalogueRepo : ICatalogueRepo
{
    private readonly CatalogueContext _context;

    public CatalogueRepo(CatalogueContext context)
    {
        _context = context;
    }

    public void CreateCatalogueItem(CatalogueItem item)
    {
        if(item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        _context.CatalogueItems.Add(item);
    }

    public IEnumerable<CatalogueItem> GetAllCatalogueItems()
    {
        return _context.CatalogueItems.ToList();
    }

    public CatalogueItem GetCatalogueItemById(int id)
    {
        return _context.CatalogueItems.FirstOrDefault(p => p.Id == id);
    }

    public bool SaveChanges()
    {
        return (_context.SaveChanges() >= 0);
    }
}
