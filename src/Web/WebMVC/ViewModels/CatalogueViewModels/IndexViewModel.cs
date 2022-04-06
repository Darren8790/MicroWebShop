using System.Collections.Generic;

namespace WebMVC.ViewModels.CatalogueViewModels;

public class IndexViewModel
{
    public IEnumerable<CatalogueItem> CatalogueItems { get; set; }
    public PaginationInfo PaginationInfo { get; set; }
}