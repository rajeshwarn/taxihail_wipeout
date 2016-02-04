<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web._default" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="RestSharp.Extensions" %>
<!DOCTYPE html>
<!--[if lt IE 7]>      <html class="no-js lt-ie9 lt-ie8 lt-ie7"> <![endif]-->
<!--[if IE 7]>         <html class="no-js lt-ie9 lt-ie8"> <![endif]-->
<!--[if IE 8]>         <html class="no-js lt-ie9"> <![endif]-->
<!--[if gt IE 8]><!--> <html class="no-js"> <!--<![endif]-->
    <head>
        <!-- version : <%: ApplicationVersion %> -->
        <meta charset='utf-8' />
        <title><%: ApplicationName %></title>
        <meta name="HandheldFriendly" content="True">
        <meta name="MobileOptimized" content="320">
        <meta name="viewport" content="width=device-width,target-densitydpi=device-dpi,initial-scale=1.0, user-scalable=no">
        <meta http-equiv="cleartype" content="on">
        <link rel="stylesheet" href='themes/<%: ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='themes/<%: ApplicationKey %>/less/combined-responsive.less'/>
        <script src="assets/js/modernizr.min.js"></script>
        <script src="https://js.braintreegateway.com/v2/braintree.js"></script>
    </head>
    <body>
        <div id="fb-root"></div>
        <header>
            <div class="login-status-zone"></div>
        </header>
        
        <div class="container">
            <div class='notification-zone'></div>
            
            <div id='main'></div>
        </div>
        <div class="map-zone"></div>
        <div class='modal-zone'></div>
        

        <!-- assets -->
        <script src="https://maps.googleapis.com/maps/api/js?client=gme-taxihailinc&v=3.19"></script>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
        
         <%: Scripts.Render("~/bundles/assets") %>
        <script src="<%: BundleTable.Bundles.ResolveBundleUrl("~/bundles/templates") %>"></script>
        <script src="taxi-hail.js"></script>
        <script src="<%: BundleTable.Bundles.ResolveBundleUrl("~/bundles/resources") %>"></script>
        
        <script type="text/javascript">
            TaxiHail.parameters.applicationName = "<%: ApplicationName %>";
            TaxiHail.parameters.applicationKey = "<%: ApplicationKey %>";
            TaxiHail.parameters.defaultLatitude = <%: DefaultLatitude %>;
            TaxiHail.parameters.defaultLongitude = <%: DefaultLongitude %>;
            TaxiHail.parameters.defaultPhoneNumber = '<%: DefaultPhoneNumber %>';
            TaxiHail.parameters.isLoggedIn = <%: IsAuthenticated ? "true" : "false" %>;
            TaxiHail.parameters.facebookAppId = '<%: FacebookAppId %>';
            TaxiHail.parameters.facebookEnabled = <%: FacebookEnabled ? "true" : "false" %>;
            TaxiHail.parameters.geolocSearchFilter = '<%= GeolocSearchFilter %>';
            TaxiHail.parameters.geolocSearchRegion = '<%= GeolocSearchRegion %>';
            TaxiHail.parameters.geolocSearchBounds = '<%= GeolocSearchBounds %>';
            TaxiHail.parameters.hideDispatchButton = <%: HideDispatchButton ? "true" : "false" %>;
            TaxiHail.parameters.showCallDriver = <%: ShowCallDriver ? "true" : "false" %>;
            TaxiHail.parameters.showMessageDriver = <%: ShowMessageDriver ? "true" : "false" %>;
            TaxiHail.parameters.accountActivationDisabled = <%: AccountActivationDisabled ? "true" : "false" %>;
            TaxiHail.parameters.isEstimateEnabled = <%: EstimateEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isEtaEnabled = <%: EtaEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isEstimateWarningEnabled = <%: EstimateWarningEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isDestinationRequired = <%: DestinationIsRequired ? "true" : "false" %>;
            TaxiHail.parameters.maxFareEstimate = <%: MaxFareEstimate %>;
            TaxiHail.parameters.disableImmediateBooking = <%: DisableImmediateBooking ? "true" : "false" %>;
            TaxiHail.parameters.disableFutureBooking = <%: DisableFutureBooking ? "true" : "false" %>;
            TaxiHail.parameters.showPassengerNumber = <%: ShowPassengerNumber ? "true" : "false" %>;
            TaxiHail.parameters.directionTarifMode = "<%: DirectionTarifMode %>";
            TaxiHail.parameters.directionNeedAValidTarif = <%: DirectionNeedAValidTarif ? "true" : "false" %>;
            TaxiHail.parameters.isChargeAccountPaymentEnabled = <%: IsChargeAccountPaymentEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isBraintreePrepaidEnabled = <%: IsBraintreePrepaidEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isCMTEnabled = <%: IsCMTEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isRideLinqCMTEnabled = <%: IsRideLinqCMTEnabled ? "true" : "false" %>;
            TaxiHail.parameters.maxNumberOfCreditCards = <%: MaxNumberOfCreditCards %>;
            TaxiHail.parameters.isPayPalEnabled = <%: IsPayPalEnabled ? "true" : "false" %>;
            TaxiHail.parameters.isCreditCardMandatory = <%: IsCreditCardMandatory ? "true" : "false" %>;
            TaxiHail.parameters.apiRoot = "api";
            TaxiHail.parameters.defaultTipPercentage = '<%= DefaultTipPercentage %>';
            TaxiHail.parameters.warnForFeesOnCancel = <%: WarnForFeesOnCancel ? "true" : "false" %>;
            TaxiHail.parameters.hideMarketChangeWarning = <%: HideMarketChangeWarning ? "true" : "false" %>;
            TaxiHail.parameters.autoConfirmFleetChange = <%: AutoConfirmFleetChange ? "true" : "false" %>;
            TaxiHail.parameters.alwaysDisplayCoFOption = <%: AlwaysDisplayCoFOption ? "true" : "false" %>;
            TaxiHail.parameters.askForCVVAtBooking = <%: AskForCVVAtBooking ? "true" : "false" %>;

            TaxiHail.parameters.isSignupVisible = <%: IsWebSignupVisible  ? "true" : "false"%>;
            TaxiHail.parameters.isSocialMediaVisible = <%: IsWebSocialMediaVisible  ? "true" : "false"%>;
            TaxiHail.parameters.SocialMediaFacebookURL = "<%: SocialMediaFacebookURL %>";
            TaxiHail.parameters.SocialMediaGoogleURL = "<%: SocialMediaGoogleURL %>";
            TaxiHail.parameters.SocialMediaPinterestURL = "<%: SocialMediaPinterestURL %>";
            TaxiHail.parameters.SocialMediaTwitterURL = "<%: SocialMediaTwitterURL %>";

            TaxiHail.parameters.availableVehicleRefreshRate = "<%: AvailableVehicleRefreshRate %>";
            TaxiHail.parameters.isSignupVisible = <%: IsWebSignupVisible  ? "true" : "false"%>;
            TaxiHail.parameters.isCraftyClicksEnabled = "<%: IsCraftyClicksEnabled ? "true" : "false" %>";
            TaxiHail.parameters.webSiteRootPath = "<%: WebSiteRootPath %>";
            TaxiHail.parameters.showOrderNumber = <%: ShowOrderNumber ? "true" : "false" %>;
            TaxiHail.parameters.isPaymentOutOfAppDisabled = "<%: IsPaymentOutOfAppDisabled %>";
            
            <% if(IsPayBackRegistrationFieldRequired == true) { %>
                TaxiHail.parameters.isPayBackRegistrationFieldRequired = "true";
            <% }
            else if(IsPayBackRegistrationFieldRequired == false) { %>
                TaxiHail.parameters.isPayBackRegistrationFieldRequired = "false";
            <% }
            else{ %>
                TaxiHail.parameters.isPayBackRegistrationFieldRequired = null;
            <% } %>

            TaxiHail.referenceData = <%= ReferenceData %>;
            TaxiHail.vehicleTypes = <%= VehicleTypes %>;
            TaxiHail.countryCodes = <%= CountryCodes %>;
            TaxiHail.parameters.defaultCountryCode = "<%= DefaultCountryCode %>";
        </script>

        <%: Scripts.Render("~/bundles/app") %>
        
        <script>
            (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
                (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
                    m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
            })(window,document,'script','//www.google-analytics.com/analytics.js','ga');

            ga('create', 'UA-44714416-2', 'auto');
            ga('send', 'pageview');

        </script>

    </body>
</html>

