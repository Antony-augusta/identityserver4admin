using System.Collections.Generic;
using FMCApp.Core.Dtos.Common;
using FMCApp.Core.Dtos.Identity.Base;

namespace FMCApp.Core.Dtos.Identity
{
    public class UserRolesDto : BaseUserRolesDto<int, int>
    {
        public UserRolesDto()
        {
           Roles = new List<RoleDto>(); 
        }
        
        public List<SelectItem> RolesList { get; set; }

        public List<RoleDto> Roles { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }
}
