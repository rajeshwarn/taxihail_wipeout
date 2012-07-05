using System;

namespace apcurium.MK.Booking.IBS
{
    public interface IBookingWebServiceClient 
    {
        //bool IsValid(ref BookingInfoData info);

        //LocationData[] SearchAddress(string address);

        //LocationData[] SearchAddress(double latitude, double longitude);

        //LocationData[] FindSimilar(string address);

        //int CreateOrder(AccountData user, BookingInfoData info, out string error);

        Tuple<string, double?, double?> GetOrderStatus(int orderId, int accountId);

        //bool IsCompleted(AccountData user, int orderId);

        //bool IsCompleted(int statusId);

        //bool CancelOrder(AccountData user, int orderId);

        //double? GetRouteDistance(double originLong, double originLat, double destLong, double destLat);

        //void UpdateHistory(AccountData user);
    }
}