using System.ComponentModel.DataAnnotations;
using FMCApp.Core.Dtos.Identity.Base;

namespace FMCApp.Core.Dtos.Identity
{
    public class RoleDto : BaseRoleDto<int>
    {      
        [Required]
        public string Name { get; set; }
    }
}