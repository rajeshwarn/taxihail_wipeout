// Geocoding service

(function () {

    TaxiHail.geocoder = {
        
        initialize: function (lat, long) {
            this.latitude = lat;
            this.longitude = long;
        },

        geocode: function (addressOrLat, lng) {

            if(arguments.length > 1) {
                // Assume parameters are latitude and longitude
                return $.get('api/geocode', { lat: addressOrLat, lng: lng }, function(){}, 'json');
            }
            else {
                // Assume parameter is an address
                return $.get('api/searchlocation', { name: addressOrLat, lat: this.latitude, lng: this.longitude }, function(){}, 'json')
                    .done(function(result){
                        if(result && result.addresses) {
                            _.each(result.addresses, function(address){
                                // BUGFIX: All addresses have the same empty Guid as id
                                delete address.id
                            });
                        }
                    });
            }
        }
    };
}());