using System.Drawing;
using CustomerPortal.Web.Properties;
using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Web.Entities.Network
{
    public class MapRegion
    {
         [Display(Name = "CoordinateStartTaxiHailNetwork", ResourceType = typeof(Resources))]
        public MapCoordinate CoordinateStart { get; set; }

         [Display(Name = "CoordinateEndTaxiHailNetwork", ResourceType = typeof(Resources))]
         public MapCoordinate CoordinateEnd { get; set; }
    
    }
}