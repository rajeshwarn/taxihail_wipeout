namespace MK.Common.iOS.Configuration
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
		void Load();
	}
}

