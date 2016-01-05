using System;
using ObjCRuntime;

namespace Braintree
{
	[Native]
	public enum BTClientMetadataSourceType : nint
	{
		Unknown = 0,
		PayPalApp,
		PayPalBrowser,
		VenmoApp,
		Form
	}

	[Native]
	public enum BTClientMetadataIntegrationType : nint
	{
		Custom,
		DropIn,
		Unknown
	}

	[Native]
	public enum BTJSONErrorCode : nint
	{
		ValueUnknown = 0,
		ValueInvalid = 1,
		AccessInvalid = 2
	}

	[Native]
	public enum BTAPIClientErrorType : nint
	{
		Unknown = 0,
		ConfigurationUnavailable
	}

	[Native]
	public enum BTAppSwitchTarget : nint
	{
		Unknown = 0,
		NativeApp,
		WebBrowser
	}

	[Native]
	public enum BTClientTokenError : nint
	{
		Unknown = 0,
		Invalid,
		UnsupportedVersion
	}

	[Native]
	public enum BTError : nint
	{
		Unknown = 0,
		CustomerInputInvalid
	}

	[Native]
	public enum BTHTTPErrorCode : nint
	{
		Unknown = 0,
		ResponseContentTypeNotAcceptable,
		ClientError,
		ServerError,
		MissingBaseURL
	}

	[Native]
	public enum BTLogLevel : nuint
	{
		None = 0,
		Critical = 1,
		Error = 2,
		Warning = 3,
		Info = 4,
		Debug = 5
	}

	[Native]
	public enum BTTokenizationServiceError : nint
	{
		Unknown = 0,
		TypeNotRegistered
	}

	[Native]
	public enum BTCardNetwork : nint
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
	public enum BTCardClientErrorType : nint
	{
		Unknown = 0,
		Integration
	}

	[Native]
	public enum BTThreeDSecureErrorType : nint
	{
		Unknown = 0,
		FailedLookup,
		FailedAuthentication,
		Integration
	}

	[Native]
	public enum BTApplePayErrorType : nint
	{
		Unknown = 0,
		Unsupported,
		Integration
	}

	[Native]
	public enum BTDataCollectorEnvironment : nint
	{
		Development,
		Qa,
		Sandbox,
		Production
	}

	[Native]
	public enum BTPayPalDriverErrorType : nint
	{
		Unknown = 0,
		Disabled,
		IntegrationReturnURLScheme,
		AppSwitchFailed,
		InvalidConfiguration,
		InvalidRequest,
		Integration
	}

	[Native]
	public enum BTUIPaymentOptionType : nint
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
		UKMaestro,
		PayPal,
		Coinbase,
		Venmo
	}

	[Native]
	public enum BTUICardFormOptionalFields : nuint
	{
		None = 0,
		Cvv = 1 << 0,
		PostalCode = 1 << 1,
		All = Cvv | PostalCode
	}

	[Native]
	public enum BTUICardFormField : nuint
	{
		Number = 0,
		Expiration,
		Cvv,
		PostalCode
	}

	[Native]
	public enum BTCardHintDisplayMode : nint
	{
		ardType,
		VVHint
	}

	[Native]
	public enum BTDropInContentViewStateType : nuint
	{
		Form = 0,
		PaymentMethodsOnFile,
		Activity
	}

	[Native]
	public enum PayPalOneTouchRequestTarget : nuint
	{
		None,
		Browser,
		OnDeviceApplication,
		Unknown
	}

	[Native]
	public enum PayPalOneTouchErrorCode : nuint
	{
		Unknown = -1000,
		ParsingFailed = -1001,
		NoTargetAppFound = -1002,
		OpenURLFailed = -1003
	}

	[Native]
	public enum PayPalOneTouchResultType : nuint
	{
		Error,
		Cancel,
		Success
	}

	[Native]
	public enum BTThreeDSecureViewControllerCompletionStatus : nint
	{
		Failure = 0,
		Success
	}
}
