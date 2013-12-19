// Geolocation service

(function(){

    var isSupported = navigator.geolocation;

    TaxiHail.geolocation = {

        initialize: function () {

            if (isSupported) {
                navigator.geolocation.watchPosition(_.bind(function (pos) {
                    this.isActive = true;
                    this.lastPosition = pos;
                }, this), _.bind(function (error) {                    
                    this.isActive = false;

                }, this), { maximumAge: 60000, timeout: 1000, enableHighAccuracy: false });
            }
            else {
                this.isActive = false;
            }
        },

        getCurrentAddress: function () {
            
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();
            
            if (this.currentAddress) {
                defer.resolve(this.currentAddress);
            }
            else {
                if (isSupported) {

                    navigator.geolocation.getCurrentPosition(_.bind(function (position) {

                        TaxiHail.geocoder.geocode(position.coords.latitude, position.coords.longitude)
                            .done(_.bind(function (result) {

                                if (result && result.length) {
                                    this.currentAddress = result[0];
                                    defer.resolve(result[0]);
                                }
                                else defer.reject();

                            }, this))
                            .fail(defer.reject);

                    }, this), defer.reject);
                }
                else {
                    window.setTimeout(defer.reject, 1);
                }
            }
            
            return defer.promise();
        },

        getCurrentPosition: function() {
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();
        
            if (this.lastPosition != null) {                
                defer.resolve(this.lastPosition.coords);
            }
            else if (isSupported) {
            
                navigator.geolocation.getCurrentPosition(_.bind(function (position) {
                    defer.resolve(position.coords);
                }, this),

                _.bind(function () {                    
                    this.isActive = false;
                    defer.reject();
                    
                }), { maximumAge: 60000, timeout: 1000, enableHighAccuracy: false }, this);
                // IE fallback
                window.setTimeout(defer.reject, 5000);
            }
            else {
                window.setTimeout(defer.reject, 1);
            }
            
            return defer.promise();
        }
    };
}());
