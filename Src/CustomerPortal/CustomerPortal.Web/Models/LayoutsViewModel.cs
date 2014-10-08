#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace CustomerPortal.Web.Models
{
    public class LayoutsViewModel
    {
        public LayoutsViewModel()
        {
            IsRejected = new Dictionary<DateTime, string>();
        }

        public string CompanyId { get; set; }
        public bool IsApproved { get; set; }
        public Dictionary<DateTime, string> IsRejected { get; set; }
        public DateTime ApprovedDate { get; set; }
        public IEnumerable<string> Layouts { get; set; }

        public bool IsEmpty
        {
            get { return !Layouts.Any(); }
        }
    }
}