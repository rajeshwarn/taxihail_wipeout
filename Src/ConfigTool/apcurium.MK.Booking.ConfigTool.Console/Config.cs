
namespace apcurium.MK.Booking.ConfigTool
{
    public abstract class Config
    {
        public Config(AppConfig parent)
        {
            Parent = parent;
        }

        public AppConfig Parent { get; set; }

        public abstract void Apply();        

    }
}
