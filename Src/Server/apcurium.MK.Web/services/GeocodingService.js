// Geocoding service

(function () {

    TaxiHail.geocoder = {
        geocode: function (address) {

            return $.get('api/searchlocation', { name: address, lat: 45.516667, lng: -73.65 }, function(){}, 'json');
        }
    };



}());