(function() {

    var Address = TaxiHail.Address = Backbone.Model.extend({ urlRoot: TaxiHail.parameters.apiRoot + '/account/addresses' });

    Address.fromGeocodingResult = function(result) {
        return new Address({
            fullAddress: result.formatted_address,
            latitude: result.geometry.location.latitude,
            longitude: result.geometry.location.longitude
        });
    };

}());