(function () {
    
    var initialLocation;

    var browserSupportFlag = new Boolean();

    TaxiHail.MapView = Backbone.View.extend({
        
        initialize: function () {
        },

        render: function() {

            var mapOptions = {
                zoom: 8,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            
            var _map = new google.maps.Map(this.el, mapOptions);

            // Try W3C Geolocation (Preferred)
            if (navigator.geolocation) {
                browserSupportFlag = true;
                navigator.geolocation.getCurrentPosition(function (position) {
                    initialLocation = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                    _map.setCenter(initialLocation);
                    
                }, function () {
                    handleNoGeolocation(browserSupportFlag);
                });
            }
                // Browser doesn't support Geolocation
            else {
                browserSupportFlag = false;
                handleNoGeolocation(browserSupportFlag);
            }

            function handleNoGeolocation(errorFlag) {
                if (errorFlag == true) {
                    alert("Geolocation service failed.");
                   // initialLocation = newyork;
                } else {
                    alert("Your browser doesn't support geolocation. We've placed you in Siberia.");
                    //initialLocation = siberia;
                }
                _map.setCenter(initialLocation);
            }
        }
        
    });

}());