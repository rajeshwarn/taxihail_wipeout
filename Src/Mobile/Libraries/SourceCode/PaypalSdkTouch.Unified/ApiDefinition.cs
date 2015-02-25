using System;
using System.Drawing;

using ObjCRuntime;
using Foundation;
using UIKit;
using CoreGraphics;

namespace Card.IO
{
    // @interface CardIOCreditCardInfo : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface CardIOCreditCardInfo  
    {
        // @property (readwrite, copy, nonatomic) NSString * cardNumber;
        [Export ("cardNumber")]
        NSString CardNumber { get; set; }

        // @property (readonly, copy, nonatomic) NSString * redactedCardNumber;
        [Export ("redactedCardNumber")]
        NSString RedactedCardNumber { get; }

        // @property (assign, readwrite, nonatomic) NSUInteger expiryMonth;
        [Export ("expiryMonth", ArgumentSemantic.UnsafeUnretained)]
        nuint ExpiryMonth { get; set; }

        // @property (assign, readwrite, nonatomic) NSUInteger expiryYear;
        [Export ("expiryYear", ArgumentSemantic.UnsafeUnretained)]
        nuint ExpiryYear { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * cvv;
        [Export ("cvv")]
        NSString Cvv { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * postalCode;
        [Export ("postalCode")]
        NSString PostalCode { get; set; }

//        // @property (readwrite, copy, nonatomic, getter = postalCode, setter = setPostalCode:) NSString * zip;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Export ("zip")]
//        NSString Zip { [Bind ("postalCode")] get; [Bind ("setPostalCode:")] set; }

        // @property (assign, readwrite, nonatomic) BOOL scanned;
        [Export ("scanned", ArgumentSemantic.UnsafeUnretained)]
        bool Scanned { get; set; }

        // @property (readwrite, nonatomic, strong) UIImage * cardImage;
        [Export ("cardImage", ArgumentSemantic.Retain)]
        UIImage CardImage { get; set; }

        // @property (readonly, assign, nonatomic) CardIOCreditCardType cardType;
        [Export ("cardType", ArgumentSemantic.UnsafeUnretained)]
        CreditCardType CardType { get; }

        // +(NSString *)displayStringForCardType:(CardIOCreditCardType)cardType usingLanguageOrLocale:(NSString *)languageOrLocale;
        [Static, Export ("displayStringForCardType:usingLanguageOrLocale:")]
        NSString DisplayStringForCardType (CreditCardType cardType, NSString languageOrLocale);

        // +(UIImage *)logoForCardType:(CardIOCreditCardType)cardType;
        [Static, Export ("logoForCardType:")]
        UIImage LogoForCardType (CreditCardType cardType);
    }

    // @protocol CardIOViewDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface CardIOViewDelegate {

        // @required -(void)cardIOView:(CardIOView *)cardIOView didScanCard:(CardIOCreditCardInfo *)cardInfo;
        [Export ("cardIOView:didScanCard:")]
        [Abstract]
        void DidScanCard (CardIOView cardIOView, CardIOCreditCardInfo cardInfo);
    }

    // @interface CardIOView : UIView
    [BaseType (typeof (UIView))]
    interface CardIOView {

        // @property (readwrite, nonatomic, weak) id<CardIOViewDelegate> delegate;
        [Export ("delegate", ArgumentSemantic.Weak)]
        [NullAllowed]
        NSObject WeakDelegate { get; set; }

        // @property (readwrite, nonatomic, weak) id<CardIOViewDelegate> delegate;
        [Wrap ("WeakDelegate")]
        CardIOViewDelegate Delegate { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * languageOrLocale;
        [Export ("languageOrLocale")]
        NSString LanguageOrLocale { get; set; }

        // @property (readwrite, retain, nonatomic) UIColor * guideColor;
        [Export ("guideColor", ArgumentSemantic.Retain)]
        UIColor GuideColor { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL useCardIOLogo;
        [Export ("useCardIOLogo", ArgumentSemantic.UnsafeUnretained)]
        bool UseCardIOLogo { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL allowFreelyRotatingCardGuide;
        [Export ("allowFreelyRotatingCardGuide", ArgumentSemantic.UnsafeUnretained)]
        bool AllowFreelyRotatingCardGuide { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * scanInstructions;
        [Export ("scanInstructions")]
        NSString ScanInstructions { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL hideCardIOLogo;
        [Export ("hideCardIOLogo", ArgumentSemantic.UnsafeUnretained)]
        bool HideCardIOLogo { get; set; }

        // @property (readwrite, retain, nonatomic) UIView * scanOverlayView;
        [Export ("scanOverlayView", ArgumentSemantic.Retain)]
        UIView ScanOverlayView { get; set; }

        // @property (assign, readwrite, nonatomic) CardIODetectionMode detectionMode;
        [Export ("detectionMode", ArgumentSemantic.UnsafeUnretained)]
        DetectionMode DetectionMode { get; set; }

        // @property (assign, readwrite, nonatomic) CGFloat scannedImageDuration;
        [Export ("scannedImageDuration", ArgumentSemantic.UnsafeUnretained)]
        nfloat ScannedImageDuration { get; set; }

        // @property (readonly, assign, nonatomic) CGRect cameraPreviewFrame;
        [Export ("cameraPreviewFrame", ArgumentSemantic.UnsafeUnretained)]
        CGRect CameraPreviewFrame { get; }

//        // @property (readwrite, copy, nonatomic) NSString * appToken;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Export ("appToken")]
//        NSString AppToken { get; set; }
    }

    // @protocol CardIOPaymentViewControllerDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface CardIOPaymentViewControllerDelegate {

        // @required -(void)userDidCancelPaymentViewController:(CardIOPaymentViewController *)paymentViewController;
        [Export ("userDidCancelPaymentViewController:")]
        [Abstract]
        void UserDidCancel (CardIOPaymentViewController paymentViewController);

        // @required -(void)userDidProvideCreditCardInfo:(CardIOCreditCardInfo *)cardInfo inPaymentViewController:(CardIOPaymentViewController *)paymentViewController;
        [Export ("userDidProvideCreditCardInfo:inPaymentViewController:")]
        [Abstract]
        void UserDidProvideCreditCardInfo (CardIOCreditCardInfo cardInfo, CardIOPaymentViewController paymentViewController);
    }

    // @interface CardIOPaymentViewController : UINavigationController
    [BaseType (typeof (UINavigationController))]
    interface CardIOPaymentViewController {

        // -(id)initWithPaymentDelegate:(id<CardIOPaymentViewControllerDelegate>)aDelegate;
        [Export ("initWithPaymentDelegate:")]
        IntPtr Constructor (CardIOPaymentViewControllerDelegate aDelegate);

        // -(id)initWithPaymentDelegate:(id<CardIOPaymentViewControllerDelegate>)aDelegate scanningEnabled:(BOOL)scanningEnabled;
        [Export ("initWithPaymentDelegate:scanningEnabled:")]
        IntPtr Constructor (CardIOPaymentViewControllerDelegate aDelegate, bool scanningEnabled);

        // @property (readwrite, copy, nonatomic) NSString * languageOrLocale;
        [Export ("languageOrLocale")]
        NSString LanguageOrLocale { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL keepStatusBarStyle;
        [Export ("keepStatusBarStyle", ArgumentSemantic.UnsafeUnretained)]
        bool KeepStatusBarStyle { get; set; }

        // @property (assign, readwrite, nonatomic) UIBarStyle navigationBarStyle;
        [Export ("navigationBarStyle", ArgumentSemantic.UnsafeUnretained)]
        UIBarStyle NavigationBarStyle { get; set; }

        // @property (readwrite, retain, nonatomic) UIColor * navigationBarTintColor;
        [Export ("navigationBarTintColor", ArgumentSemantic.Retain)]
        UIColor NavigationBarTintColor { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL disableBlurWhenBackgrounding;
        [Export ("disableBlurWhenBackgrounding", ArgumentSemantic.UnsafeUnretained)]
        bool DisableBlurWhenBackgrounding { get; set; }

        // @property (readwrite, retain, nonatomic) UIColor * guideColor;
        [Export ("guideColor", ArgumentSemantic.Retain)]
        UIColor GuideColor { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL suppressScanConfirmation;
        [Export ("suppressScanConfirmation", ArgumentSemantic.UnsafeUnretained)]
        bool SuppressScanConfirmation { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL suppressScannedCardImage;
        [Export ("suppressScannedCardImage", ArgumentSemantic.UnsafeUnretained)]
        bool SuppressScannedCardImage { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL maskManualEntryDigits;
        [Export ("maskManualEntryDigits", ArgumentSemantic.UnsafeUnretained)]
        bool MaskManualEntryDigits { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * scanInstructions;
        [Export ("scanInstructions")]
        NSString ScanInstructions { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL hideCardIOLogo;
        [Export ("hideCardIOLogo", ArgumentSemantic.UnsafeUnretained)]
        bool HideCardIOLogo { get; set; }

        // @property (readwrite, retain, nonatomic) UIView * scanOverlayView;
        [Export ("scanOverlayView", ArgumentSemantic.Retain)]
        UIView ScanOverlayView { get; set; }

        // @property (assign, readwrite, nonatomic) CardIODetectionMode detectionMode;
        [Export ("detectionMode", ArgumentSemantic.UnsafeUnretained)]
        DetectionMode DetectionMode { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL collectExpiry;
        [Export ("collectExpiry", ArgumentSemantic.UnsafeUnretained)]
        bool CollectExpiry { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL collectCVV;
        [Export ("collectCVV", ArgumentSemantic.UnsafeUnretained)]
        bool CollectCVV { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL collectPostalCode;
        [Export ("collectPostalCode", ArgumentSemantic.UnsafeUnretained)]
        bool CollectPostalCode { get; set; }

//        // @property (assign, readwrite, nonatomic, getter = collectPostalCode, setter = setCollectPostalCode:) BOOL collectZip;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Export ("collectZip", ArgumentSemantic.UnsafeUnretained)]
//        bool CollectZip { [Bind ("collectPostalCode")] get; [Bind ("setCollectPostalCode:")] set; }

        // @property (assign, readwrite, nonatomic) BOOL useCardIOLogo;
        [Export ("useCardIOLogo", ArgumentSemantic.UnsafeUnretained)]
        bool UseCardIOLogo { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL allowFreelyRotatingCardGuide;
        [Export ("allowFreelyRotatingCardGuide", ArgumentSemantic.UnsafeUnretained)]
        bool AllowFreelyRotatingCardGuide { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL disableManualEntryButtons;
        [Export ("disableManualEntryButtons", ArgumentSemantic.UnsafeUnretained)]
        bool DisableManualEntryButtons { get; set; }

        // @property (readwrite, nonatomic, weak) id<CardIOPaymentViewControllerDelegate> paymentDelegate;
        [Export ("paymentDelegate", ArgumentSemantic.Weak)]
        [NullAllowed]
        NSObject WeakPaymentDelegate { get; set; }

        // @property (readwrite, nonatomic, weak) id<CardIOPaymentViewControllerDelegate> paymentDelegate;
        [Wrap ("WeakPaymentDelegate")]
        CardIOPaymentViewControllerDelegate PaymentDelegate { get; set; }

//        // @property (assign, readwrite, nonatomic) BOOL showsFirstUseAlert;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Export ("showsFirstUseAlert", ArgumentSemantic.UnsafeUnretained)]
//        bool ShowsFirstUseAlert { get; set; }

//        // @property (readwrite, copy, nonatomic) NSString * appToken;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Export ("appToken")]
//        NSString AppToken { get; set; }
    }

    // @interface CardIOUtilities : NSObject
    [BaseType (typeof (NSObject))]
    interface CardIOUtilities 
    {
        // +(NSString *)libraryVersion;
        [Static, Export ("libraryVersion")]
        NSString LibraryVersion ();

        // +(BOOL)canReadCardWithCamera;
        [Static, Export ("canReadCardWithCamera")]
        bool CanReadCardWithCamera ();

        // +(void)preload;
        [Static, Export ("preload")]
        void Preload ();

        // +(UIImageView *)blurredScreenImageView;
        [Static, Export ("blurredScreenImageView")]
        UIImageView BlurredScreenImageView ();
    }
}

namespace PaypalSdkTouch.Unified
{
    // @interface PayPalConfiguration : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface PayPalConfiguration {

        // @property (readwrite, copy, nonatomic) NSString * defaultUserEmail;
        [Export ("defaultUserEmail")]
        NSString DefaultUserEmail { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * defaultUserPhoneCountryCode;
        [Export ("defaultUserPhoneCountryCode")]
        NSString DefaultUserPhoneCountryCode { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * defaultUserPhoneNumber;
        [Export ("defaultUserPhoneNumber")]
        NSString DefaultUserPhoneNumber { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * merchantName;
        [Export ("merchantName")]
        NSString MerchantName { get; set; }

        // @property (readwrite, copy, nonatomic) NSURL * merchantPrivacyPolicyURL;
        [Export ("merchantPrivacyPolicyURL", ArgumentSemantic.Copy)]
        NSUrl MerchantPrivacyPolicyURL { get; set; }

        // @property (readwrite, copy, nonatomic) NSURL * merchantUserAgreementURL;
        [Export ("merchantUserAgreementURL", ArgumentSemantic.Copy)]
        NSUrl MerchantUserAgreementURL { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL acceptCreditCards;
        [Export ("acceptCreditCards", ArgumentSemantic.UnsafeUnretained)]
        bool AcceptCreditCards { get; set; }

        // @property (assign, readwrite, nonatomic) PayPalShippingAddressOption payPalShippingAddressOption;
        [Export ("payPalShippingAddressOption", ArgumentSemantic.UnsafeUnretained)]
        PayPalShippingAddressOption PayPalShippingAddressOption { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL rememberUser;
        [Export ("rememberUser", ArgumentSemantic.UnsafeUnretained)]
        bool RememberUser { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * languageOrLocale;
        [Export ("languageOrLocale")]
        NSString LanguageOrLocale { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL disableBlurWhenBackgrounding;
        [Export ("disableBlurWhenBackgrounding", ArgumentSemantic.UnsafeUnretained)]
        bool DisableBlurWhenBackgrounding { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL presentingInPopover;
        [Export ("presentingInPopover", ArgumentSemantic.UnsafeUnretained)]
        bool PresentingInPopover { get; set; }

        // @property (assign, readwrite, nonatomic) BOOL forceDefaultsInSandbox;
        [Export ("forceDefaultsInSandbox", ArgumentSemantic.UnsafeUnretained)]
        bool ForceDefaultsInSandbox { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * sandboxUserPassword;
        [Export ("sandboxUserPassword")]
        NSString SandboxUserPassword { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * sandboxUserPin;
        [Export ("sandboxUserPin")]
        NSString SandboxUserPin { get; set; }
    }

    // @protocol PayPalFuturePaymentDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface PayPalFuturePaymentDelegate {

        // @required -(void)payPalFuturePaymentDidCancel:(PayPalFuturePaymentViewController *)futurePaymentViewController;
        [Export ("payPalFuturePaymentDidCancel:")]
        [Abstract]
        void DidCancelFuturePayment (PayPalFuturePaymentViewController futurePaymentViewController);

        // @required -(void)payPalFuturePaymentViewController:(PayPalFuturePaymentViewController *)futurePaymentViewController didAuthorizeFuturePayment:(NSDictionary *)futurePaymentAuthorization;
        [Export ("payPalFuturePaymentViewController:didAuthorizeFuturePayment:")]
        [Abstract]
        void DidAuthorizeFuturePayment (PayPalFuturePaymentViewController futurePaymentViewController, NSDictionary futurePaymentAuthorization);

        // @optional -(void)payPalFuturePaymentViewController:(PayPalFuturePaymentViewController *)futurePaymentViewController willAuthorizeFuturePayment:(NSDictionary *)futurePaymentAuthorization completionBlock:(PayPalFuturePaymentDelegateCompletionBlock)completionBlock;
        [Export ("payPalFuturePaymentViewController:willAuthorizeFuturePayment:completionBlock:")]
        void WillAuthorizeFuturePayment (PayPalFuturePaymentViewController futurePaymentViewController, NSDictionary futurePaymentAuthorization, Action completionBlock);
    }

    // @interface PayPalFuturePaymentViewController : UINavigationController
    [BaseType (typeof (UINavigationController))]
    interface PayPalFuturePaymentViewController {

        // -(instancetype)initWithConfiguration:(PayPalConfiguration *)configuration delegate:(id<PayPalFuturePaymentDelegate>)delegate;
        [Export ("initWithConfiguration:delegate:")]
        IntPtr Constructor (PayPalConfiguration configuration, PayPalFuturePaymentDelegate futurePaymentDelegate);

        // @property (readonly, nonatomic, weak) id<PayPalFuturePaymentDelegate> futurePaymentDelegate;
        [Export ("futurePaymentDelegate", ArgumentSemantic.Weak)]
        [NullAllowed]
        NSObject WeakFuturePaymentDelegate { get; }

        // @property (readonly, nonatomic, weak) id<PayPalFuturePaymentDelegate> futurePaymentDelegate;
        [Wrap ("WeakFuturePaymentDelegate")]
        PayPalFuturePaymentDelegate FuturePaymentDelegate { get; }
    }

    // @interface PayPalPaymentDetails : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface PayPalPaymentDetails  {

        // @property (readwrite, copy, nonatomic) NSDecimalNumber * subtotal;
        [Export ("subtotal", ArgumentSemantic.Copy)]
        NSDecimalNumber Subtotal { get; set; }

        // @property (readwrite, copy, nonatomic) NSDecimalNumber * shipping;
        [Export ("shipping", ArgumentSemantic.Copy)]
        NSDecimalNumber Shipping { get; set; }

        // @property (readwrite, copy, nonatomic) NSDecimalNumber * tax;
        [Export ("tax", ArgumentSemantic.Copy)]
        NSDecimalNumber Tax { get; set; }

        // +(PayPalPaymentDetails *)paymentDetailsWithSubtotal:(NSDecimalNumber *)subtotal withShipping:(NSDecimalNumber *)shipping withTax:(NSDecimalNumber *)tax;
        [Static, Export ("paymentDetailsWithSubtotal:withShipping:withTax:")]
        PayPalPaymentDetails PaymentDetailsWithSubtotal (NSDecimalNumber subtotal, NSDecimalNumber shipping, NSDecimalNumber tax);
    }

    // @interface PayPalItem : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface PayPalItem  {

        // @property (readwrite, copy, nonatomic) NSString * name;
        [Export ("name")]
        NSString Name { get; set; }

        // @property (assign, readwrite, nonatomic) NSUInteger quantity;
        [Export ("quantity", ArgumentSemantic.UnsafeUnretained)]
        nuint Quantity { get; set; }

        // @property (readwrite, copy, nonatomic) NSDecimalNumber * price;
        [Export ("price", ArgumentSemantic.Copy)]
        NSDecimalNumber Price { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * currency;
        [Export ("currency")]
        NSString Currency { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * sku;
        [Export ("sku")]
        NSString Sku { get; set; }

        // +(PayPalItem *)itemWithName:(NSString *)name withQuantity:(NSUInteger)quantity withPrice:(NSDecimalNumber *)price withCurrency:(NSString *)currency withSku:(NSString *)sku;
        [Static, Export ("itemWithName:withQuantity:withPrice:withCurrency:withSku:")]
        PayPalItem ItemWithName (NSString name, nuint quantity, NSDecimalNumber price, NSString currency, NSString sku);

        // +(NSDecimalNumber *)totalPriceForItems:(NSArray *)items;
        [Static, Export ("totalPriceForItems:")]
        NSDecimalNumber TotalPriceForItems (NSObject [] items);
    }

    // @interface PayPalShippingAddress : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface PayPalShippingAddress  {

        // @property (readwrite, copy, nonatomic) NSString * recipientName;
        [Export ("recipientName")]
        NSString RecipientName { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * line1;
        [Export ("line1")]
        NSString Line1 { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * line2;
        [Export ("line2")]
        NSString Line2 { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * city;
        [Export ("city")]
        NSString City { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * state;
        [Export ("state")]
        NSString State { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * postalCode;
        [Export ("postalCode")]
        NSString PostalCode { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * countryCode;
        [Export ("countryCode")]
        NSString CountryCode { get; set; }

        // +(PayPalShippingAddress *)shippingAddressWithRecipientName:(NSString *)recipientName withLine1:(NSString *)line1 withLine2:(NSString *)line2 withCity:(NSString *)city withState:(NSString *)state withPostalCode:(NSString *)postalCode withCountryCode:(NSString *)countryCode;
        [Static, Export ("shippingAddressWithRecipientName:withLine1:withLine2:withCity:withState:withPostalCode:withCountryCode:")]
        PayPalShippingAddress ShippingAddressWithRecipientName (NSString recipientName, NSString line1, NSString line2, NSString city, NSString state, NSString postalCode, NSString countryCode);
    }

    // @interface PayPalPayment : NSObject <NSCopying>
    [BaseType (typeof (NSObject))]
    interface PayPalPayment  {

        // @property (readwrite, copy, nonatomic) NSString * currencyCode;
        [Export ("currencyCode")]
        NSString CurrencyCode { get; set; }

        // @property (readwrite, copy, nonatomic) NSDecimalNumber * amount;
        [Export ("amount", ArgumentSemantic.Copy)]
        NSDecimalNumber Amount { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * shortDescription;
        [Export ("shortDescription")]
        NSString ShortDescription { get; set; }

        // @property (assign, readwrite, nonatomic) PayPalPaymentIntent intent;
        [Export ("intent", ArgumentSemantic.UnsafeUnretained)]
        PayPalPaymentIntent Intent { get; set; }

        // @property (readwrite, copy, nonatomic) PayPalPaymentDetails * paymentDetails;
        [Export ("paymentDetails", ArgumentSemantic.Copy)]
        PayPalPaymentDetails PaymentDetails { get; set; }

        // @property (readwrite, copy, nonatomic) NSArray * items;
        [Export ("items", ArgumentSemantic.Copy)]
        NSObject [] Items { get; set; }

        // @property (readwrite, copy, nonatomic) PayPalShippingAddress * shippingAddress;
        [Export ("shippingAddress", ArgumentSemantic.Copy)]
        PayPalShippingAddress ShippingAddress { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * invoiceNumber;
        [Export ("invoiceNumber")]
        NSString InvoiceNumber { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * custom;
        [Export ("custom")]
        NSString Custom { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * softDescriptor;
        [Export ("softDescriptor")]
        NSString SoftDescriptor { get; set; }

        // @property (readwrite, copy, nonatomic) NSString * bnCode;
        [Export ("bnCode")]
        NSString BnCode { get; set; }

        // @property (readonly, assign, nonatomic) BOOL processable;
        [Export ("processable", ArgumentSemantic.UnsafeUnretained)]
        bool Processable { get; }

        // @property (readonly, copy, nonatomic) NSString * localizedAmountForDisplay;
        [Export ("localizedAmountForDisplay")]
        NSString LocalizedAmountForDisplay { get; }

        // @property (readonly, copy, nonatomic) NSDictionary * confirmation;
        [Export ("confirmation", ArgumentSemantic.Copy)]
        NSDictionary Confirmation { get; }

        // +(PayPalPayment *)paymentWithAmount:(NSDecimalNumber *)amount currencyCode:(NSString *)currencyCode shortDescription:(NSString *)shortDescription intent:(PayPalPaymentIntent)intent;
        [Static, Export ("paymentWithAmount:currencyCode:shortDescription:intent:")]
        PayPalPayment PaymentWithAmount (NSDecimalNumber amount, NSString currencyCode, NSString shortDescription, PayPalPaymentIntent intent);
    }

    // @protocol PayPalPaymentDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface PayPalPaymentDelegate {

        // @required -(void)payPalPaymentDidCancel:(PayPalPaymentViewController *)paymentViewController;
        [Export ("payPalPaymentDidCancel:")]
        [Abstract]
        void PayPalPaymentDidCancel (PayPalPaymentViewController paymentViewController);

        // @required -(void)payPalPaymentViewController:(PayPalPaymentViewController *)paymentViewController didCompletePayment:(PayPalPayment *)completedPayment;
        [Export ("payPalPaymentViewController:didCompletePayment:")]
        [Abstract]
        void DidCompletePayment (PayPalPaymentViewController paymentViewController, PayPalPayment completedPayment);

        // @optional -(void)payPalPaymentViewController:(PayPalPaymentViewController *)paymentViewController willCompletePayment:(PayPalPayment *)completedPayment completionBlock:(PayPalPaymentDelegateCompletionBlock)completionBlock;
        [Export ("payPalPaymentViewController:willCompletePayment:completionBlock:")]
        void WillCompletePayment (PayPalPaymentViewController paymentViewController, PayPalPayment completedPayment, Action completionBlock);
    }

    // @interface PayPalPaymentViewController : UINavigationController
    [BaseType (typeof (UINavigationController))]
    interface PayPalPaymentViewController {

        // -(instancetype)initWithPayment:(PayPalPayment *)payment configuration:(PayPalConfiguration *)configuration delegate:(id<PayPalPaymentDelegate>)delegate;
        [Export ("initWithPayment:configuration:delegate:")]
        IntPtr Constructor (PayPalPayment payment, PayPalConfiguration configuration, PayPalPaymentDelegate paymentDelegate);

        // @property (readonly, nonatomic, weak) id<PayPalPaymentDelegate> paymentDelegate;
        [Export ("paymentDelegate", ArgumentSemantic.Weak)]
        [NullAllowed]
        NSObject WeakPaymentDelegate { get; }

        // @property (readonly, nonatomic, weak) id<PayPalPaymentDelegate> paymentDelegate;
        [Wrap ("WeakPaymentDelegate")]
        PayPalPaymentDelegate PaymentDelegate { get; }

        // @property (readonly, assign, nonatomic) PayPalPaymentViewControllerState state;
        [Export ("state", ArgumentSemantic.UnsafeUnretained)]
        PayPalPaymentViewControllerState State { get; }
    }

    // @protocol PayPalProfileSharingDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface PayPalProfileSharingDelegate {

        // @required -(void)userDidCancelPayPalProfileSharingViewController:(PayPalProfileSharingViewController *)profileSharingViewController;
        [Export ("userDidCancelPayPalProfileSharingViewController:")]
        [Abstract]
        void UserDidCancelPayPalProfileSharingViewController (PayPalProfileSharingViewController profileSharingViewController);

        // @required -(void)payPalProfileSharingViewController:(PayPalProfileSharingViewController *)profileSharingViewController userDidLogInWithAuthorization:(NSDictionary *)profileSharingAuthorization;
        [Export ("payPalProfileSharingViewController:userDidLogInWithAuthorization:")]
        [Abstract]
        void UserDidLogInWithAuthorization (PayPalProfileSharingViewController profileSharingViewController, NSDictionary profileSharingAuthorization);

        // @optional -(void)payPalProfileSharingViewController:(PayPalProfileSharingViewController *)profileSharingViewController userWillLogInWithAuthorization:(NSDictionary *)profileSharingAuthorization completionBlock:(PayPalProfileSharingDelegateCompletionBlock)completionBlock;
        [Export ("payPalProfileSharingViewController:userWillLogInWithAuthorization:completionBlock:")]
        void UserWillLogInWithAuthorization (PayPalProfileSharingViewController profileSharingViewController, NSDictionary profileSharingAuthorization, Action completionBlock);
    }

    // @interface PayPalProfileSharingViewController : UINavigationController
    [BaseType (typeof (UINavigationController))]
    interface PayPalProfileSharingViewController {

        // -(instancetype)initWithScopeValues:(NSSet *)scopeValues configuration:(PayPalConfiguration *)configuration delegate:(id<PayPalProfileSharingDelegate>)delegate;
        [Export ("initWithScopeValues:configuration:delegate:")]
        IntPtr Constructor (NSSet scopeValues, PayPalConfiguration configuration, PayPalProfileSharingDelegate profileSharingDelegate);

        // @property (readonly, nonatomic, weak) id<PayPalProfileSharingDelegate> profileSharingDelegate;
        [Export ("profileSharingDelegate", ArgumentSemantic.Weak)]
        [NullAllowed]
        NSObject WeakProfileSharingDelegate { get; }

        // @property (readonly, nonatomic, weak) id<PayPalProfileSharingDelegate> profileSharingDelegate;
        [Wrap ("WeakProfileSharingDelegate")]
        PayPalProfileSharingDelegate ProfileSharingDelegate { get; }
    }

    // @interface PayPalMobile : NSObject
    [BaseType (typeof (NSObject))]
    interface PayPalMobile {

        // +(void)initializeWithClientIdsForEnvironments:(NSDictionary *)clientIdsForEnvironments;
        [Static, Export ("initializeWithClientIdsForEnvironments:")]
        void InitializeWithClientIdsForEnvironments (NSDictionary clientIdsForEnvironments);

        // +(void)preconnectWithEnvironment:(NSString *)environment;
        [Static, Export ("preconnectWithEnvironment:")]
        void PreconnectWithEnvironment (NSString environment);

        // +(NSString *)clientMetadataID;
        [Static, Export ("clientMetadataID")]
        NSString ClientMetadataID ();

//        // +(NSString *)applicationCorrelationIDForEnvironment:(NSString *)environment;
//        [Availability (Deprecated = Platform.iOS | Platform.Mac)]
//        [Static, Export ("applicationCorrelationIDForEnvironment:")]
//        NSString ApplicationCorrelationIDForEnvironment (NSString environment);

        // +(void)clearAllUserData;
        [Static, Export ("clearAllUserData")]
        void ClearAllUserData ();

        // +(NSString *)libraryVersion;
        [Static, Export ("libraryVersion")]
        NSString LibraryVersion ();

        [Field ("PayPalEnvironmentProduction", "__Internal")]
        NSString PayPalEnvironmentProduction { get; }

        [Field ("PayPalEnvironmentSandbox", "__Internal")]
        NSString PayPalEnvironmentSandbox { get; }

        [Field ("PayPalEnvironmentNoNetwork", "__Internal")]
        NSString PayPalEnvironmentNoNetwork { get; }    
    }

}

