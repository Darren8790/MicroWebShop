using Microsoft.AspNetCore.Mvc;
using WebMVC.Services;
using WebMVC.ViewModels;
using WebMVC.ViewModels.CatalogueViewModels;

namespace WebMVC.Controllers;

public class CatalogueController : Controller
{
    private ICatalogueService _catalogueSvc;

    public CatalogueController(ICatalogueService catalogueSvc)
    {
        _catalogueSvc = catalogueSvc;
    }

    public async Task<IActionResult> Index(int? page) //int? page
    {
        Console.WriteLine("--> Controller hit");
        var itemsPage = 10;
        var catalogue = await _catalogueSvc.GetCatalogueItems(page ?? 0, itemsPage); //page ?? 0, itemsPage
        var viewModel = new IndexViewModel()
        {
            CatalogueItems = catalogue.Data,
            PaginationInfo = new PaginationInfo()
            {
                ActualPage = page ?? 0,
                ItemsPerPage = catalogue.Data.Count,
                TotalItems = catalogue.Count,
                TotalPages = (int)Math.Ceiling(((decimal)catalogue.Count / itemsPage))
            }
        };

        viewModel.PaginationInfo.Next = (viewModel.PaginationInfo.ActualPage == viewModel.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
        viewModel.PaginationInfo.Previous = (viewModel.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";
        return View(viewModel);
    }
}