(function () {
    
    var initialLocation;

    var browserSupportFlag = new Boolean();
    var map;
    TaxiHail.MapView = Backbone.View.extend({
        
        initialize: function () {
        },
            
            
render: function() {

            var mapOptions = {
                zoom: 8,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            map = new google.maps.Map(this.el, mapOptions);
            var pos;
            
            this.model.on('change:pickupAddress', function (model, value) {
                this.addMarker(new google.maps.LatLng(value.latitude, value.longitude), map, 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                this.addMarker(new google.maps.LatLng(value.latitude, value.longitude),map,'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
            }, this);

            this.geolocalize();
            
            

        },
        
        geolocalize : function () {
            // Try W3C Geolocation (Preferred)
            if (navigator.geolocation) {
                browserSupportFlag = true;
                navigator.geolocation.getCurrentPosition(function (position) {
                    initialLocation = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);

                    map.setCenter(initialLocation);
                    marker = new google.maps.Marker({
                        position: initialLocation,
                        map: map

                    });

                }, function () {
                    handleNoGeolocation(browserSupportFlag);
                });
            }
                // Browser doesn't support Geolocation
            else {
                browserSupportFlag = false;
                handleNoGeolocation(browserSupportFlag);
            }
        },
        
            addMarker : function(location, mapc, iconImage) {
                    marker = new google.maps.Marker({
            position: location,
            map: mapc,
            icon : iconImage
            });
                
        },

        handleNoGeolocation : function (errorFlag) {
            if (errorFlag == true) {
                alert("Geolocation service failed.");
                // initialLocation = newyork;
            } else {
                alert("Your browser doesn't support geolocation. We've placed you in Siberia.");
                //initialLocation = siberia;
            }
            map.setCenter(initialLocation);
        }
        
    });

}());