// Geocoding service

(function (maps) {
    TaxiHail.geocoder = {
        
        initialize: function (lat, lng) {
            this.latitude = lat;
            this.longitude = lng;
            this.geocoder = new maps.Geocoder();
        },

        geocode: function (lat, lng) {
            var defer = $.Deferred();
            
            this.geocoder.geocode({ latLng: new maps.LatLng(lat, lng) }, function (results, status) {
                if (status == maps.GeocoderStatus.OK) {

                    $.ajax({
                        url: TaxiHail.parameters.apiRoot + '/geocode',
                        type: "POST",
                        data: JSON.stringify({ lat: lat, lng: lng, geoResult: results }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json"
                    }).then(defer.resolve, defer.reject);
                    
                } else {
                    defer.reject();
                }
            });

            return defer.then(cleanupResult).promise();

        },

        search: function(address) {

            var defaultLatitude = this.latitude,
                defaultLongitude = this.longitude,
                // Check if first character is numeric                
                geoResult = $.when();
                        
                var filtered = TaxiHail.parameters.geolocSearchFilter.replace('{0}', address);
                    
                geoResult = $.get(url, {
                    address: filtered,
                    region: TaxiHail.parameters.geolocSearchRegion,
                    bounds: TaxiHail.parameters.geolocSearchBounds,
                    sensor: true
                });


            if (TaxiHail.geolocation.isActive) {
            
                return TaxiHail.geolocation.getCurrentPosition()
               .pipe(function (coords) {
                   return geoResult.pipe(function (geoResult) {
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
            else {

                return geoResult.pipe(function (geoResult) {
                    return search(address, {
                        latitude: defaultLatitude,
                        longitude: defaultLongitude
                    }, geoResult);
                });
                
            }


            

           
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
}(google.maps));