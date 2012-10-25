// Geolocation service

(function(){

    var isSupported = navigator.geolocation;

    TaxiHail.geolocation = {

        getCurrentAddress: function () {
            
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();
            
            if (this.currentAddress) {
                defer.resolve(this.currentAddress);
            }else {
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
        
            if (isSupported) {

                navigator.geolocation.getCurrentPosition(_.bind(function (position) {

                    defer.resolve(position.coords);

                }, this), defer.reject);
            }
            else {
                window.setTimeout(defer.reject, 1);
            }
            
            return defer.promise();
        }
    };
}());
