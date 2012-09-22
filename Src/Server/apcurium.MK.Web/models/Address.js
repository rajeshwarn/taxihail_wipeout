(function() {

    var Address = TaxiHail.Address = Backbone.Model.extend({ });

    Address.fromGeocodingResult = function(result)
    {
        return new Address({
            fullAddress: result.formatted_address,
            latitude: result.geometry.location.latitude,
            longitude: result.geometry.location.longitude
        })
    };

}());