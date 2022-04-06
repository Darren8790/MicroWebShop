using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebMVC.ViewModels;

public class AppUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
}