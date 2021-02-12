using Codex.BackOffice.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Codex.BackOffice.Models.Security
{
    public class ApiKeyModel
    {
        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "ID", ResourceType = typeof(AppResource))]
        public string? Id { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "NAME", ResourceType = typeof(AppResource))]
        public string? Name { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED_FIELD_ERROR", ErrorMessageResourceType = typeof(AppResource))]
        [Display(Name = "ROLES", ResourceType = typeof(AppResource))]
        public List<SelectData> Roles { get; set; } = new List<SelectData>();
    }

    public class SelectData
    {
        public SelectData(string name, bool selected)
            => (Name, Selected) = (name, selected);

        public string Name { get; set; }

        public bool Selected { get; set; }
    }
}
