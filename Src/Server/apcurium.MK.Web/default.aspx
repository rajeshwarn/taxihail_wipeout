<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="apcurium.MK.Web._default" %>

<!DOCTYPE html>
<html>
    <head>
        <meta charset='utf-8' />
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
        <title>Taxi Hail</title>
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined.less'/>
        <link rel="stylesheet" href='themes/<%: this.ApplicationKey %>/less/combined-responsive.less'/>
        <script src="assets/js/modernizr.js"></script>
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
        <script src="assets/js/persist-min.js"></script>
        <script src="assets/js/handlebars-1.0.rc.1.js"></script>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
        <script src="assets/js/jquery.hotkeys.js"></script>
        <script src="assets/js/jquery.edit-in-place.js"></script>
        <script src="assets/js/jquery.validate.js"></script>
        <script src="assets/js/underscore.js"></script>
        <script src="assets/js/backbone.js"></script>
        <script src="assets/bootstrap/js/bootstrap-alert.js"></script>
        <script src="assets/bootstrap/js/bootstrap-transition.js"></script>
        <script src="assets/bootstrap/js/bootstrap-button.js"></script>
        <script src="assets/bootstrap/js/bootstrap-modal.js"></script>
        <script src="assets/bootstrap/js/bootstrap-dropdown.js"></script>
        <script src="assets/bootstrap/js/bootstrap-tooltip.js"></script>
        <script src="assets/js/bootstrap-tooltip.js"></script>
        <script src="assets/bootstrap/js/bootstrap-popover.js"></script>
        <script src="assets/bootstrap-datepicker/js/bootstrap-datepicker.js"></script>
        <script src="assets/bootstrap-timepicker/js/bootstrap-timepicker.js"></script>

        <script src="assets/js/spin.js"></script>
        
        <script src="https://maps.googleapis.com/maps/api/js?sensor=false"></script>
        
        <!-- app -->
        <script src="taxi-hail.js"></script>
        
        <script type="text/javascript">
            TaxiHail.parameters.defaultLatitude = <%: this.DefaultLatitude %>;
            TaxiHail.parameters.defaultLongitude = <%: this.DefaultLongitude %>;
            TaxiHail.parameters.isLoggedIn = <%: this.IsAuthenticated ? "true" : "false" %>;
        </script>

        <script src="utils.js"></script>


        <!-- mixins -->
        <script src="mixins/ValidatedView.js"></script>

        <!-- models -->
        <script src="models/Address.js"></script>
        <script src="models/Order.js"></script>
        <script src="models/OrderStatus.js"></script>
        <script src="models/UserAccount.js"></script>
        <script src="models/Settings.js"></script>
        <script src="models/NewAccount.js"></script>
        <script src="models/ReferenceData.js"></script>
        <!-- collections -->
        <script src="collections/AddressCollection.js"></script>
        <script src="collections/OrderCollection.js"></script>
        <!-- views -->
        <script src="views/TemplatedView.js"></script>
        <script src="views/BookView.js"></script>
        <script src="views/BookLaterView.js"></script>
        <script src="views/LoginView.js"></script>
        <script src="views/AddressControlView.js"></script>
        <script src="views/AddressSelectionView.js"></script>
        <script src="views/AddressListView.js"></script>
        <script src="views/FavoritesAndHistoryListView.js"></script>
        <script src="views/AddressItemView.js"></script>
        <script src="views/BookingConfirmationView.js"></script>
        <script src="views/BookingStatusView.js"></script>
        <script src="views/MapView.js"></script>
        <script src="views/SignupView.js"></script>
        <script src="views/LoginStatusView.js"></script>
        <script src="views/UserAccountView.js"></script>
        <script src="views/ProfileView.js"></script>
        <script src="views/UpdatePasswordView.js"></script>
        <script src="views/ResetPasswordView.js"></script>
        <script src="views/OrderHistoryView.js"></script>
        <script src="views/OrderHistoryDetailView.js"></script>
        <script src="views/OrderItemView.js"></script>
        <script src="views/BootstrapConfirmationView.js"></script>
        <script src="views/FavoriteDetailsView.js"></script>
        <script src="views/AddFavoriteView.js"></script>
        <script src="views/FavoritesView.js"></script>
        <!-- services -->
        <script src="services/AuthService.js"></script>
        <script src="services/OrderService.js"></script>
        <script src="services/GeocodingService.js"></script>
        <script src="services/GeolocationService.js"></script>
        <script src="services/PlacesService.js"></script>
        <script src="services/DirectionInfoService.js"></script>
        
        <script src="routers/App.js"></script>

    </body>
</html>

