<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web.admin._default" %>

<!DOCTYPE html>
<html>
    <head>
        <meta charset='utf-8' />
        <title><%: ApplicationName %>  - Administration</title>
        <meta name="HandheldFriendly" content="True">
        <meta name="MobileOptimized" content="320">
        <meta name="viewport" content="width=device-width,target-densitydpi=device-dpi,initial-scale=1.0, user-scalable=no">
        <meta http-equiv="cleartype" content="on">
        <link rel="stylesheet" href='../themes/<%: this.ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='../themes/<%: this.ApplicationKey %>/less/combined-responsive.less'/>
        <script src="../assets/js/modernizr.min.js"></script>
    </head>
    <body>
        
        <header>
            <div class="login-status-zone"></div>
        </header>
        
        <div class="pull-left menuadmin"></div>
        
        <div class="container">
            <div class='notification-zone'></div>
            
            <div id='main'></div>
        </div>
        <div class='modal-zone'></div>
        

        <!-- assets -->
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
        
        <%= JSAssetsSource %>
        <script src="templates/templates.js"></script>

        <script src="taxi-hail-admin.js"></script>
        
        <script type="text/javascript">
            TaxiHail.parameters.isLoggedIn = <%: this.IsAuthenticated ? "true" : "false" %>;
            TaxiHail.parameters.apiRoot = "../api";
        </script>

        <%= JSAppSource %>
    </body>
</html>

