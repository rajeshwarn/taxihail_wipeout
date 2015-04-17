
using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class MarketModel
    {
        [Display(Name = "Name")]
        public string Market { get; set; }
    }
}