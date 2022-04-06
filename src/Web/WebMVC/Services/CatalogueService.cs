using System.Text.Json;
using Microsoft.Extensions.Options;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public class CatalogueService : ICatalogueService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogueService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOptions<AppSettings> _settings;
    private readonly string _remoteServiceBaseUrl;

    public CatalogueService(HttpClient httpClient, ILogger<CatalogueService> logger, IConfiguration configuration, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _settings = settings;

        _remoteServiceBaseUrl = $"{_settings.Value.PurchaseUrl}{_configuration["CatalogueService"]}/api/v1/catalogue/";
        //_remoteServiceBaseUrl = $"{_settings.Value.PurchaseUrl}/c/api/v1/catalogue/";
    }
    
    // public async Task<Catalogue> GetCatalogueItems()
    // {
    //     var baseAddress = $"{_configuration["CatalogueService"]}/api/v1/catalogue/items/";
    //     var response = await _httpClient.GetStringAsync(baseAddress);
    //     var msg = JsonSerializer.Deserialize<Catalogue>(response);
    //     return msg;
    // }

    public async Task<Catalogue> GetCatalogueItems(int page, int take) //int page, int take
    {
        Console.WriteLine("--> catalogue service");

        var uri = API.Catalogue.GetAllCatalogueItems(_remoteServiceBaseUrl, page, take); //page, take
        var responseString = await _httpClient.GetStringAsync(uri);
        var catalogueItems = JsonSerializer.Deserialize<Catalogue>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return catalogueItems;
    }
}