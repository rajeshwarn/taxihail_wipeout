<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web._default" %>
<%@ Import Namespace="System.Web.Optimization" %>

<!DOCTYPE html>
<html>
    <head>
        <meta charset='utf-8' />
        <title><%: ApplicationName %></title>
        <meta name="HandheldFriendly" content="True">
        <meta name="MobileOptimized" content="320">
        <meta name="viewport" content="width=device-width,target-densitydpi=device-dpi,initial-scale=1.0, user-scalable=no">
        <meta http-equiv="cleartype" content="on">
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined-responsive.less'/>
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
            TaxiHail.parameters.defaultLatitude = <%: this.DefaultLatitude %>;
            TaxiHail.parameters.defaultLongitude = <%: this.DefaultLongitude %>;
            TaxiHail.parameters.defaultPhoneNumber = '<%: this.DefaultPhoneNumber %>';
            TaxiHail.parameters.isLoggedIn = <%: this.IsAuthenticated ? "true" : "false" %>;
            TaxiHail.parameters.facebookAppId = '<%: this.FacebookAppId %>';
            TaxiHail.parameters.facebookEnabled = <%: this.FacebookEnabled %>;
            TaxiHail.parameters.apiRoot = "api";
        </script>

        <%: Scripts.Render("~/bundles/app") %>

    </body>
</html>

