(function () {

    TaxiHail.CompanyDefaultAddressCollection = Backbone.Collection.extend({
        model: TaxiHail.CompanyDefaultAddress,
        url: TaxiHail.parameters.apiRoot + '/admin/addresses'
    });

}());