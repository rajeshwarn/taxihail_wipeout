(function () {
    
    var ServiceType = TaxiHail.ServiceType = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/servicetypes',
        idAttribute: "serviceType"
   });

}());