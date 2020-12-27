using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Users
{
    public class UserCreator
    {
        [Required]
        [MinLength(2, ErrorMessage = "The Login field must be a minimum of 2 characters")]
        [StringLength(20, ErrorMessage = "Name length can't be more than 20.")]
        public string? Login { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name length can't be more than 100.")]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name length can't be more than 100.")]
        public string? LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "Name length can't be more than 20.")]
        [MinLength(6, ErrorMessage = "The Password field must be a minimum of 6 characters")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "re-type password is required.")]
        [Compare(nameof(Password), ErrorMessage = "password dosent match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string? TenantId { get; set; }
    }
}
