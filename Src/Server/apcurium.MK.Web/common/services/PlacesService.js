// Places service

(function () {

    TaxiHail.places = {
        
        search: function (lat, lng) {

            return $.get(TaxiHail.parameters.apiRoot + '/places', { lat: lat, lng: lng }, function () {}, 'json')
                .done(TaxiHail.cleanupAddressesResult);
        },

        getPlaceDetails: function (placeId, placeName) {
            return $.get(TaxiHail.parameters.apiRoot + '/places/detail', { PlaceId: placeId, PlaceName: placeName }, function () { }, 'json')
                .done(TaxiHail.cleanupAddressesResult);
        }
    };
}());