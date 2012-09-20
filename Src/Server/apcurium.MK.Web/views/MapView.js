(function () {
    
    var initialLocation;

    var browserSupportFlag = new Boolean();
    var map;
    var pickupPin;
    var dropOffPin;
    TaxiHail.MapView = Backbone.View.extend({
        
        initialize: function () {
            this.model.on('change:pickupAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                if (pickupPin) {
                    pickupPin.setPosition(location);
                } else {
                    pickupPin = this.addMarker(location, map, 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');
                }
                map.setCenter(location);
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                if (dropOffPin) {
                    dropOffPin.setPosition(location);
                } else {
                    dropOffPin = this.addMarker(location, map, 'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
                }
            }, this);
        },
           
            
        render: function() {

            var mapOptions = {
                zoom: 12,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                scrollwheel: false,

            };
            map = new google.maps.Map(this.el, mapOptions);

            this.geolocalize();
            
            var _map = new google.maps.Map(this.el, mapOptions);


        },
        
        geolocalize : function () {
            // Try W3C Geolocation (Preferred)
            if (navigator.geolocation) {
                browserSupportFlag = true;
                navigator.geolocation.getCurrentPosition(function (position) {
                    initialLocation = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);

                    map.setCenter(initialLocation);
                    pickupPin = new google.maps.Marker({
                        position: initialLocation,
                        map: map,
                        icon : 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'

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
                    icon: iconImage
                } );
                return marker;
        },

        handleNoGeolocation : function (errorFlag) {
            if (errorFlag == true) {
                alert(TaxiHail.localize('Geolocation service failed.'));
                // initialLocation = newyork;
            } else {
                alert(TaxiHail.localize('Your browser doesn\'t support geolocation. We\'ve placed you in Siberia.'));
                //initialLocation = siberia;
            }
            map.setCenter(initialLocation);
        }
        }
        
    });

}());