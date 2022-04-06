using AutoMapper;
using CatalogueService.Dtos;
using CatalogueService.Model;

namespace CatalogueService.Profiles;

public class CatalogueProfile : Profile
{
    public CatalogueProfile()
    {
        // Source -> Target
        CreateMap<CatalogueItem, CatalogueReadDto>();
        CreateMap<CatalogueCreateDto, CatalogueItem>();
    }
}