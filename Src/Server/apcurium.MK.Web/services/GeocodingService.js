// Geocoding service

(function () {

    TaxiHail.geocoder = {
        geocode: function (address) {

            return $.get('api/geocode', { name: address }, function(){}, 'json');
        }
    };



}());