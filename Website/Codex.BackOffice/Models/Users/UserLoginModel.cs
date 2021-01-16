using Codex.BackOffice.Resources;
using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Users
{
    public class UserLoginModel
    {
        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "USER_NAME", ResourceType = typeof(AppResource))]
        public string? Login { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "PASSWORD", ResourceType = typeof(AppResource))]
        public string? Password { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "TENANT", ResourceType = typeof(AppResource))]
        public string? TenantId { get; set; }
    }
}
