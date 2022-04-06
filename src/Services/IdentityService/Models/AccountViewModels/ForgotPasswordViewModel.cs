using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.AccountViewModels
{
    public record ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }
    }
}