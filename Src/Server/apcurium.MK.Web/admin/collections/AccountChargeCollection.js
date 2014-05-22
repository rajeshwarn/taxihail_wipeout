(function(){

    TaxiHail.AccountChargeCollection = Backbone.Collection.extend({
        model: TaxiHail.AccountCharge,
        url: TaxiHail.parameters.apiRoot + '/admin/accountscharge'
    });
}());