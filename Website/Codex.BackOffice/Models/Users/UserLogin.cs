using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Users
{
    public record UserLogin
    {
        [Required]
        public string? Login { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? TenantId { get; set; }
    }
}
