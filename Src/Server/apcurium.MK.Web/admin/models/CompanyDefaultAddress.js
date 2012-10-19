(function () {

    var CompanyDefaultAddress = TaxiHail.CompanyDefaultAddress = Backbone.Model.extend({ urlRoot: '../api/admin/addresses' });

    CompanyDefaultAddress.fromGeocodingResult = function (result) {
        return new CompanyDefaultAddress({
            fullAddress: result.formatted_address,
            latitude: result.geometry.location.latitude,
            longitude: result.geometry.location.longitude
        });
    };

}());