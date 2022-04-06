using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.AccountViewModels
{
    public record RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; }
        [Required]
        [StringLength(100, ErrorMessage ="The {0} must be at least {2} and at max {1} characters long.", MinimumLength =6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; init; }
        public AppUser User { get; init; }
    }
}