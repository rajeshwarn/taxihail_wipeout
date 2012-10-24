(function () {

    var CompanyPopularAddress = TaxiHail.CompanyPopularAddress = Backbone.Model.extend({ urlRoot: '../api/admin/popularaddresses' });

    CompanyPopularAddress.fromGeocodingResult = function (result) {
        return new CompanyPopularAddress({
            fullAddress: result.formatted_address,
            latitude: result.geometry.location.latitude,
            longitude: result.geometry.location.longitude
        });
    };

}());