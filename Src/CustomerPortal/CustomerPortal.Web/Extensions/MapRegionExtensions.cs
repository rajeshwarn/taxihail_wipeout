using System;
using System.Drawing;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Extensions
{
    public static class MapRegionExtensions
    {
        public static RectangleF GetRectangle(this MapRegion thisRegion)
        {

            return new RectangleF(
                Math.Min((float)thisRegion.CoordinateStart.Latitude, (float)thisRegion.CoordinateEnd.Latitude),
                Math.Min((float)thisRegion.CoordinateStart.Longitude, (float)thisRegion.CoordinateEnd.Longitude),
                Math.Abs((float)thisRegion.CoordinateEnd.Latitude - (float)thisRegion.CoordinateStart.Latitude),
                Math.Abs((float)thisRegion.CoordinateEnd.Longitude - (float)thisRegion.CoordinateStart.Longitude)
                );
        }

        public static bool Contains(this MapRegion thisRegion, MapRegion region)
        {
            var myRect = thisRegion.GetRectangle();
            var otherRect = region.GetRectangle();
            var intersect = myRect.IntersectsWith(otherRect);

            return intersect;
        }

public static bool Contains(this MapRegion thisRegion, MapCoordinate position)
        {
            var myRect = thisRegion.GetRectangle();
            var positionInRegion = myRect.Contains((float)position.Latitude, (float)position.Longitude);

            return positionInRegion;
        }
    }
}