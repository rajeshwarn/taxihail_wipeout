using System;
using ObjCRuntime;

namespace BraintreeBindings
{
	[Native]
	public enum BTClientMetadataSourceType : long
	{
		Unknown = 0,
		PayPalApp,
		PayPalBrowser,
		VenmoApp,
		Form
	}

	[Native]
	public enum BTClientMetadataIntegrationType : long
	{
		Custom,
		DropIn,
		Unknown
	}

	[Native]
	public enum BTJSONErrorCode : int
	{
		ValueUnknown = 0,
		ValueInvalid = 1,
		AccessInvalid = 2
	}

	[Native]
	public enum BTAPIClientErrorType : int
	{
		Unknown = 0,
		ConfigurationUnavailable
	}

	[Native]
	public enum BTAppSwitchTarget : long
	{
		Unknown = 0,
		NativeApp,
		WebBrowser
	}

	[Native]
	public enum BTClientTokenError : int
	{
		Unknown = 0,
		Invalid,
		UnsupportedVersion
	}

	[Native]
	public enum BTError : int
	{
		Unknown = 0,
		CustomerInputInvalid
	}

	[Native]
	public enum BTHTTPErrorCode : int
	{
		Unknown = 0,
		ResponseContentTypeNotAcceptable,
		ClientError,
		ServerError,
		MissingBaseURL
	}

	[Native]
	public enum BTLogLevel : ulong
	{
		None = 0,
		Critical = 1,
		Error = 2,
		Warning = 3,
		Info = 4,
		Debug = 5
	}

	[Native]
	public enum BTTokenizationServiceError : int
	{
		Unknown = 0,
		TypeNotRegistered
	}

	[Native]
	public enum BTApplePayErrorType : int
	{
		Unknown = 0,
		Unsupported,
		Integration
	}

	[Native]
	public enum BTCardNetwork : int
	{
		Unknown = 0,
		Amex,
		DinersClub,
		Discover,
		MasterCard,
		Visa,
		Jcb,
		Laser,
		Maestro,
		UnionPay,
		Solo,
		Switch,
		UKMaestro
	}

	[Native]
	public enum BTCardClientErrorType : int
	{
		Unknown = 0,
		Integration
	}

	[Native]
	public enum BTPayPalDriverErrorType : int
	{
		Unknown = 0,
		Disabled,
		IntegrationReturnURLScheme,
		AppSwitchFailed,
		InvalidConfiguration,
		InvalidRequest,
		Integration
	}
}