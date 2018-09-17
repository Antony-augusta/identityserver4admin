using FMCApp.Core.Dtos.Identity.Base;

namespace FMCApp.Core.Dtos.Identity
{
    public class UserProviderDto : BaseUserProviderDto<int>
    {
        public string ProviderKey { get; set; }

        public string LoginProvider { get; set; }

        public string ProviderDisplayName { get; set; }        
    }
}
