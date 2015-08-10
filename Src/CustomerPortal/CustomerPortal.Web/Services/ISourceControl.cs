using CustomerPortal.Web.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Web.Services
{
    public interface ISourceControl
    {
        Task<bool> UpdateVersions();
        bool IsVersionNumber(Revision revision);
    }
}
