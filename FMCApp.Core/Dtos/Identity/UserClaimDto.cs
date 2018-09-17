using System.ComponentModel.DataAnnotations;
using FMCApp.Core.Dtos.Identity.Base;

namespace FMCApp.Core.Dtos.Identity
{
    public class UserClaimDto : BaseUserClaimDto<int, int>
    {
        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }
    }
}