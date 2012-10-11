<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web._default" %>

<!DOCTYPE html>
<html>
    <head>
        <meta charset='utf-8' />
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
        <title><%: ApplicationName %></title>
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined-responsive.less'/>
        <script src="assets/js/modernizr.custom.20404.js"></script>
    </head>
    <body>
        <header>
            <div class="login-status-zone"></div>
        </header>
        <div id="fb-root"></div>
        <div class="container">
            <div class='notification-zone'></div>
            
            <div id='main'></div>
        </div>
        <div class="map-zone"></div>
        <div class='modal-zone'></div>
        

        <!-- assets -->
        <script src="https://maps.googleapis.com/maps/api/js?sensor=false"></script>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
        
        <%= JSAssetsSource %>
        <script src="templates.js"></script>

        <script src="taxi-hail.js"></script>
        
        <script type="text/javascript">
            TaxiHail.parameters.defaultLatitude = <%: this.DefaultLatitude %>;
            TaxiHail.parameters.defaultLongitude = <%: this.DefaultLongitude %>;
            TaxiHail.parameters.isLoggedIn = <%: this.IsAuthenticated ? "true" : "false" %>;
        </script>

        <%= JSAppSource %>

    </body>
</html>

