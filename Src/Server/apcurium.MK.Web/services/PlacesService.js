// Places service

(function () {

    TaxiHail.places = {
        
        search: function (lat, lng) {

            return $.get('api/places', { lat: lat, lng: lng }, function () {}, 'json')
                .done(cleanupResult);
        },

        getPlaceDetails: function(reference) {
            return $.get('api/places/' + reference, {}, function () {}, 'json')
                .done(cleanupResult);
        }
    };

    function cleanupResult(results) {
        if(_.isArray(results)) {
            _.each(results, function(address){
                // BUGFIX: All addresses have the same empty Guid as id
                delete address.id;
            });
        }
    }
}());