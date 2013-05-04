// Geocoding service

(function () {
    var url = "https://maps.googleapis.com/maps/api/geocode/json";
    TaxiHail.geocoder = {
        
        initialize: function (lat, lng) {
            this.latitude = lat;
            this.longitude = lng;
        },

        geocode: function (lat, lng) {

            return $.get(url, {
                latlng: lat + ',' + lng,
                sensor: true
            }, function () { }, 'json')
                .pipe(function (geoResult) {
                    return $.ajax({
                        url:TaxiHail.parameters.apiRoot + '/geocode',
                        type:"POST",
                        data:JSON.stringify({ lat: lat, lng: lng, geoResult: geoResult }),
                        contentType:"application/json; charset=utf-8",
                        dataType:"json"
                    }).then(cleanupResult);
                });

        },

        search: function(address) {

            var defaultLatitude = this.latitude,
                defaultLongitude = this.longitude,
                // Check if first character is numeric
                isNumeric = !_.isNaN(parseInt(address.substring(0, 1), 10)),
                geoResult = $.when();

            // Geocode address if address starts by a digit
            if (isNumeric) {
                var filtered = TaxiHail.parameters.geolocSearchFilter.replace('{0}', address);
                    
                geoResult = $.get(url, {
                    address: filtered,
                    region: TaxiHail.parameters.geolocSearchRegion,
                    sensor: true
                });
            }

            return TaxiHail.geolocation.getCurrentPosition()
                .pipe(function (coords) {
                    return geoResult.pipe(function(geoResult) {
                        return search(address, coords, geoResult);
                    });
                    
                }, function () {
                    return geoResult.pipe(function (geoResult) {
                        return search(address, {
                            latitude: defaultLatitude,
                            longitude: defaultLongitude
                        }, geoResult);
                    });
                    
                });
        }
    };

    function search(address, coords, geoResult) {
        return $.ajax({
            url:TaxiHail.parameters.apiRoot + '/searchlocation',
            type:"POST",
            data:JSON.stringify({
                name: address,
                lat: coords.latitude,
                lng: coords.longitude,
                geoResult: geoResult
            }),
            contentType:"application/json; charset=utf-8",
            dataType:"json"
        }).done(cleanupResult);
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