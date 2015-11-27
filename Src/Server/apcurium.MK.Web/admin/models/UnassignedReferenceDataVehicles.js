(function () {

    TaxiHail.UnassignedReferenceDataVehicles = Backbone.Collection.extend({
        initialize: function (models, options) {
            this.serviceType = options.serviceType;
        },
        url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/unassignedreference/?serviceType=' + serviceType
});

}());