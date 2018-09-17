using System;
using System.Collections.Generic;
using System.Text;
using FMCApp.Core.Helpers;

namespace FMCApp.Core.Resources
{
    public interface IIdentityResourceServiceResources
    {
        ResourceMessage IdentityResourceDoesNotExist();

        ResourceMessage IdentityResourceExistsKey();

        ResourceMessage IdentityResourceExistsValue();
    }
}
