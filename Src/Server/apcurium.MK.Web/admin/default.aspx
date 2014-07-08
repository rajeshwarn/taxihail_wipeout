<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web.admin._default" %>
<%@ Import Namespace="System.Web.Optimization" %>

<!DOCTYPE html>
<html>
    <head>
        <meta charset='utf-8' />
        <title><%: ApplicationName %>  - Administration</title>
        <meta name="HandheldFriendly" content="True">
        <meta name="MobileOptimized" content="320">
        <meta name="viewport" content="width=device-width,target-densitydpi=device-dpi,initial-scale=1.0, user-scalable=no">
        <meta http-equiv="cleartype" content="on">
        <link rel="stylesheet" href='../themes/<%: ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='assets/less/TaxiHail-admin-grid.less'/>
        <link rel="stylesheet" href='assets/css/admin.css'/>
        <link rel="stylesheet" href='../themes/<%: ApplicationKey %>/less/combined-responsive.less'/>
        <script src="../assets/js/modernizr.min.js"></script>
    </head>
    <body>
        
        <header>
            <div class="login-status-zone">
                <div><a href='../#' class='brand'></a></div>
            </div>
        </header>
        
        
        <div class="container">
            <div class='notification-zone'></div>
            <div class='row'>
                <div class="menu-zone span3"></div>
                <div id='main' class='span9'></div>
            </div>
        </div>
        <div class='modal-zone'></div>
        

        <!-- assets -->
        <script src="https://maps.googleapis.com/maps/api/js?sensor=false"></script>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
        
        <%: Scripts.Render("~/admin/bundles/assets") %>
        <script src="<%: BundleTable.Bundles.ResolveBundleUrl("~/admin/bundles/templates") %>"></script>
        <script src="taxi-hail-admin.js"></script>
        <script src="<%: BundleTable.Bundles.ResolveBundleUrl("~/admin/bundles/resources") %>"></script>
        
        <script type="text/javascript">
            TaxiHail.parameters.defaultLatitude = <%: DefaultLatitude %>;
            TaxiHail.parameters.defaultLongitude = <%: DefaultLongitude %>;
            TaxiHail.parameters.isLoggedIn = <%: IsAuthenticated ? "true" : "false" %>;
            TaxiHail.parameters.isSuperAdmin = <%: IsSuperAdmin ? "true" : "false" %>;
            TaxiHail.parameters.geolocSearchFilter = '<%= GeolocSearchFilter %>';
            TaxiHail.parameters.geolocSearchRegion = '<%= GeolocSearchRegion %>';
            TaxiHail.parameters.geolocSearchBounds = '<%= GeolocSearchBounds %>';
            TaxiHail.parameters.apiRoot = "../api";
            TaxiHail.parameters.version = '<%: ApplicationVersion %>';
        </script>

        <%: Scripts.Render("~/admin/bundles/app") %>
    </body>
</html>

