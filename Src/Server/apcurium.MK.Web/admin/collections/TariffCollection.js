(function(){

    TaxiHail.TariffCollection = Backbone.Collection.extend({
        model: TaxiHail.Tariff,
        url: TaxiHail.parameters.apiRoot + '/admin/tariffs'
    });

}());