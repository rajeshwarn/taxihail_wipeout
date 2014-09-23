<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web._default" %>
<%@ Import Namespace="System.Web.Optimization" %>
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
        <script src="https://maps.googleapis.com/maps/api/js?sensor=false"></script>
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
            TaxiHail.parameters.facebookEnabled = <%: FacebookEnabled %>;
            TaxiHail.parameters.geolocSearchFilter = '<%= GeolocSearchFilter %>';
            TaxiHail.parameters.geolocSearchRegion = '<%= GeolocSearchRegion %>';
            TaxiHail.parameters.geolocSearchBounds = '<%= GeolocSearchBounds %>';
            TaxiHail.parameters.hideDispatchButton = <%: HideDispatchButton %>;
            TaxiHail.parameters.accountActivationDisabled = <%: AccountActivationDisabled %>;
            TaxiHail.parameters.isEstimateEnabled = <%: EstimateEnabled %>;
            TaxiHail.parameters.isEtaEnabled = <%: EtaEnabled %>;
            TaxiHail.parameters.isEstimateWarningEnabled = <%: EstimateWarningEnabled %>;
            TaxiHail.parameters.isDestinationRequired = <%: DestinationIsRequired %>;
            TaxiHail.parameters.maxFareEstimate = <%: MaxFareEstimate %>;
            TaxiHail.parameters.disableFutureBooking = <%: DisableFutureBooking %>;
            TaxiHail.parameters.showPassengerNumber = <%: ShowPassengerNumber ? "true" : "false" %>;
            TaxiHail.parameters.directionTarifMode = "<%: DirectionTarifMode %>";
            TaxiHail.parameters.directionNeedAValidTarif = <%: DirectionNeedAValidTarif ? "true" : "false" %>;
            TaxiHail.parameters.apiRoot = "api";
            
            TaxiHail.parameters.isSignupVisible = <%: IsWebSignupVisible  ? "true" : "false"%>;

            TaxiHail.referenceData = <%= ReferenceData %>;
            TaxiHail.vehicleTypes = <%= VehicleTypes %>;
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

