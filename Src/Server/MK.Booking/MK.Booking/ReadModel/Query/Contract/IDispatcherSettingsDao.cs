namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IDispatcherSettingsDao
    {
        DispatcherSettingsDetail GetSettings(string market);
    }
}