// Geocoding service

(function () {

    var geocoder = new google.maps.Geocoder();

    TaxiHail.geocoder = {
        geocode: function (address) {

            var defer = $.Deferred();

            geocoder.geocode({ 'address': address}, function(results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    defer.resolve(results, status);
                  } else {
                    defer.reject(results, status);
                  }
                });
            
            return defer.promise();
        }
    };

}());