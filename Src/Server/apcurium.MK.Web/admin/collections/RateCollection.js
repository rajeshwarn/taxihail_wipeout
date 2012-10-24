(function(){

    TaxiHail.RateCollection = Backbone.Collection.extend({
        model: TaxiHail.Rate,
        url: TaxiHail.parameters.apiRoot + '/rates'
    });

}());