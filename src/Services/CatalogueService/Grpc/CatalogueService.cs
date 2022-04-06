using CatalogueService.Data;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace CatalogueService.Grpc;

public class CatalogueService
{
    private readonly CatalogueContext _catalogueContext;
    private readonly IOptions<CatalogueSettings> _settings;
    private readonly ILogger<CatalogueService> _logger;

    public CatalogueService(CatalogueContext dbContext, IOptions<CatalogueSettings> settings, ILogger<CatalogueService> logger)
    {
        _catalogueContext = dbContext;
        _settings = settings;
        _logger = logger;
    }

    // public override async Task<CatalogueItemResponse> GetItemById(CatalogueItemRequest request, ServerCallContext context)
    // {

    // }
}