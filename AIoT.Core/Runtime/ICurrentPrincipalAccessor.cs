using System;
using System.Security.Claims;

namespace AIoT.Core.Runtime
{
    public interface ICurrentPrincipalAccessor
    {
        ClaimsPrincipal Principal { get; }

        IDisposable Change(ClaimsPrincipal principal);
    }
}
