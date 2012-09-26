(function(){

    TaxiHail.OrderStatus = Backbone.Model.extend({

        url: function() {
            return 'api/account/orders/' + this.id + '/status/';
        }

    });


}());