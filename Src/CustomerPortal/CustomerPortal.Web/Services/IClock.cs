#region

using System;

#endregion

namespace CustomerPortal.Web.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}