using Codex.BackOffice.Resources;
using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Users
{
    public class UserInfoModel
    {
        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [StringLength(100, ErrorMessageResourceName = "MINIMAL_STRING_LENGTH", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "FIRST_NAME", ResourceType = typeof(AppResource))]
        public string? FirstName { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [StringLength(100, ErrorMessageResourceName = "MINIMAL_STRING_LENGTH", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "LAST_NAME", ResourceType = typeof(AppResource))]
        public string? LastName { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "LANGUAGE", ResourceType = typeof(AppResource))]
        public string? LanguageCultureName { get; set; }
    }
}
