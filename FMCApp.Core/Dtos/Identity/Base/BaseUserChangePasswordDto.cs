namespace FMCApp.Core.Dtos.Identity.Base
{
    public class BaseUserChangePasswordDto<TUserId>
    {
        public TUserId UserId { get; set; }
    }
}