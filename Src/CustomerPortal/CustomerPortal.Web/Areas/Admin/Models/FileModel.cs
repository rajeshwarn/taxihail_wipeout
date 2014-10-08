#region

using System.Collections.Generic;
using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class FileModel
    {
        public IEnumerable<string> Files { get; set; }

        public string CompanyName { get; set; }

        public Company Company { get; set; }
    }
}