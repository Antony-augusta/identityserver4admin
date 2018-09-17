namespace FMCApp.Core.Dtos.Identity.Base
{
    public class BaseUserClaimDto<TUserId, TClaimId>
    {
        public TClaimId ClaimId { get; set; }

        public TUserId UserId { get; set; }
    }
}