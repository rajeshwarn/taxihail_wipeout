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
                return $.get('api/searchlocation', { name: addressOrLat, lat: 45.516667, lng: -73.65 }, function(){}, 'json');
            }
        },

    };



}());