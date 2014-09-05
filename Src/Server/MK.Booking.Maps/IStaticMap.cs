using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Maps
{
    public interface IStaticMap
    {
        string GetStaticMapUri(Position pickup, Position dropOff, float width, float height, float scale);
    }
}