using Codex.BackOffice.Resources;
using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Users
{
    public class UserPasswordModel
    {
        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessageResourceName = "MINIMAL_STRING_LENGTH", ErrorMessageResourceType = typeof(AppResource), MinimumLength = 6)]
        [Display(Name = "PASSWORD", ResourceType = typeof(AppResource))]
        public string? Password { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Compare(nameof(Password), ErrorMessageResourceName = "PASSWORD_DOES_NOT_MATCH", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "CONFIRM_PASSWORD", ResourceType = typeof(AppResource))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
