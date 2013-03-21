(function(){

    TaxiHail.RuleCollection = Backbone.Collection.extend({
        model: TaxiHail.Rule,
        url: TaxiHail.parameters.apiRoot + '/admin/rules'
    });
}());