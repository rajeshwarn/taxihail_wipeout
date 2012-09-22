// Geolocation service

(function(){

    var isSupported = navigator.geolocation;

    TaxiHail.geolocation = {
        getCurrentPosition: function () {
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();

            if (isSupported) {

                navigator.geolocation.getCurrentPosition(function(position) {

                    TaxiHail.geocoder.geocode(position.coords.latitude, position.coords.longitude)
                        .done(function(result) {

                            if(result.addresses && result.addresses.length) {
                                defer.resolve(result.addresses[0]);
                            }
                            else defer.reject();

                        })
                        .fail(defer.reject);

                }, defer.reject);
            }
            else {
                window.setTimeout(defer.reject, 1);
            }

            return defer.promise();
        }
    };
}());
