namespace WebMVC.Services.DTOs;

public record BasketDTO
{
    public string Street { get; init; }
    public string City { get; init; }
    public string Country { get; init; }
    public string PostCode { get; init; }
    public string Buyer { get; init; }
    public Guid RequestId { get; init; }
}