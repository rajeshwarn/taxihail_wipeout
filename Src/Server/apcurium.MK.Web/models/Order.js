(function () {

    TaxiHail.Order = Backbone.Model.extend({

        idAttribute: 'orderId',
        url: 'api/account/orders'

    });

}());