// Geocoding service

(function () {
    var url = "https://maps.googleapis.com/maps/api/geocode/json";
    TaxiHail.geocoder = {
        
        initialize: function (lat, lng) {
            this.latitude = lat;
            this.longitude = lng;
        },

        geocode: function (lat, lng) {

            var result = $.Deferred();
            $.get(url, {
                latlng: lat + ',' + lng,
                sensor: true
            }, function () { }, 'json')
                .then(function (geoResult) {
                    $.post(TaxiHail.parameters.apiRoot + '/geocode', { lat: lat, lng: lng, geoResult: geoResult }, function () { }, 'json')
                        .then(cleanupResult)
                        .then(result.resolve, result.reject);
                });

            return result.promise();

        },

        search: function(address) {

            var defaultLatitude = this.latitude,
                defaultLongitude = this.longitude;

            return TaxiHail.geolocation.getCurrentPosition()
                .pipe(function(coords) {
                    return search(address, coords);
                }, function() {
                    return search(address, {
                        latitude: defaultLatitude,
                        longitude: defaultLongitude
                    });
                });
        }
    };

    function search(address, coords) {
        return $.get(TaxiHail.parameters.apiRoot + '/searchlocation',  {
            name: address,
            lat: coords.latitude,
            lng: coords.longitude
        }, function () { }, 'json').done(cleanupResult);
    }

    function cleanupResult(result) {
        if(result && result.length) {
            _.each(result, function(address){
                // BUGFIX: All addresses have the same empty Guid as id
                delete address.id;
            });
        }
    }
}());