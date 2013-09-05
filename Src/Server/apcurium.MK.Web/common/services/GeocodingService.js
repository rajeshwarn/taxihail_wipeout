// Geocoding service

(function (maps) {
    TaxiHail.geocoder = {
        
        initialize: function (lat, lng) {
            this.latitude = lat;
            this.longitude = lng;
            this.geocoder = new maps.Geocoder();
            this.bounds = getBounds();
        },

        geocode: function (lat, lng) {
            var defer = $.Deferred();
            
            this.geocoder.geocode({ latLng: new maps.LatLng(lat, lng) }, function (results, status) {
                if (status == maps.GeocoderStatus.OK) {
                    


                    $.ajax({
                        url: TaxiHail.parameters.apiRoot + '/geocode',
                        type: "POST",
                        data: JSON.stringify({ lat: lat, lng: lng, geoResult: { results: fixResults(results), status: status } }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json"
                    }).then(defer.resolve, defer.reject);
                    
                } else {
                    defer.reject();
                }
            });

            return defer.promise().then(cleanupResult);

        },

        search: function(address) {

            var geocodeDefer = $.Deferred(), // Deferred for the geocoding request
                geolocDefer = $.Deferred(), // Deferred for the geolocation request
                defaultLatitude = this.latitude,
                defaultLongitude = this.longitude;

            var filtered = TaxiHail.parameters.geolocSearchFilter.replace('{0}', address);

            TaxiHail.geolocation.getCurrentPosition()
                .then(geolocDefer.resolve, function () {
                    // If geoloc failed, resolve geolocDefer with default coordinates
                    geolocDefer.resolve({
                        latitude: defaultLatitude,
                        longitude: defaultLongitude
                    });
                });

            this.geocoder.geocode({
                address: filtered,
                region: TaxiHail.parameters.geolocSearchRegion,
                bounds: this.bounds
            }, function(results, status) {
                if (status == maps.GeocoderStatus.OK) {

                    geocodeDefer.resolve({
                        results: fixResults(results),
                        status: status
                    });
                } else {
                    geocodeDefer.reject();
                }
            });

            var result = $.when(geolocDefer.promise() /*always resolves*/, geocodeDefer.promise()).pipe(function (geoloc, geocode) {
                return search(address, geoloc, geocode);
            });
            return result;

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
    
    function getBounds() {
        var bounds = null,
            param = TaxiHail.parameters.geolocSearchBounds,
            hasBounds = param && param.indexOf('|') > 0;
        
        if (hasBounds) {
            var c = param.split(/[,|]/);
            bounds = new maps.LatLngBounds(new maps.LatLng(c[0], c[1]), maps.LatLng(c[2], c[3]));
        }
        return bounds;
    }
    
    function fixResults(results) {
        if (results && results.length) {
            // Transform LatLng object to plain js objects
            // For JSON serialization
            _.each(results, function (result) {
                result.geometry.location = {
                    lat: result.geometry.location.lat(),
                    lng: result.geometry.location.lng()
                };
            });
        }
        return results;
    }
}(google.maps));