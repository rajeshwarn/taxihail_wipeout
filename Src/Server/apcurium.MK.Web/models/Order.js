﻿(function () {

    TaxiHail.Order = Backbone.Model.extend({

        idAttribute: 'orderId',
        urlRoot: 'api/account/orders',

        cancel: function() {

            return $.post(this.url() + '/cancel', {
                orderId: this.id
            }, function(){}, 'json');

        },
        getStatus: function() {
            return this._status || (this._status = new TaxiHail.OrderStatus({
                id: this.id
            }));
        }
    });

}());