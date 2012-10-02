(function(){

    TaxiHail.OrderStatus = Backbone.Model.extend({

        url: function() {
            return 'api/account/orders/' + this.id + '/status/';
        },

        isActive: function() {
            var status = this.get('iBSStatusId');
            return _.indexOf(['wosCANCELLED', 'wosCANCELLED_DONE', 'wosDONE', 'wosLOADED'], status) == -1;
        }
    });


}());