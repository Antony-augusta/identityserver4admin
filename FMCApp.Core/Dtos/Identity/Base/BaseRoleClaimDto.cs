namespace FMCApp.Core.Dtos.Identity.Base
{
    public class BaseRoleClaimDto<TRoleId, TClaimId>
    {
        public TClaimId ClaimId { get; set; }

        public TRoleId RoleId { get; set; }
    }
}