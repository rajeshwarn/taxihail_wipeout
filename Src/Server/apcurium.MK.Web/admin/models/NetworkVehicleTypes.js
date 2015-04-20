(function () {

    TaxiHail.NetworkVehicleTypes = Backbone.Collection.extend({
        url: TaxiHail.parameters.customerPortalUrl + 'customer/marketVehicleTypes?companyId=' + TaxiHail.parameters.applicationKey
    });

}());