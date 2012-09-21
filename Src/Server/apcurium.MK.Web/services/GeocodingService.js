// Geocoding service

(function () {

    TaxiHail.geocoder = {
        geocode: function (addressOrLat, lng) {

            if(arguments.length > 1) {
                // Assume parameters are latitude and longitude
                return $.get('api/geocode', { lat: addressOrLat, lng: lng }, function(){}, 'json');
            }
            else {
                // Assume parameter is an address
                return $.get('api/searchlocation', { name: addressOrLat, lat: 45.516667, lng: -73.65 }, function(){}, 'json')
                .done(function(result){
                    if(result && result.addresses) {
                        _.each(result.addresses, function(address){
                            // BUGFIX: All addresses have the same empty Guid as id
                            delete address.id
                        });
                    }
                });
            }
        },

    };



}());