(function(){

    TaxiHail.OrderStatus = Backbone.Model.extend({

        url: function() {
            return 'api/account/orders/' + this.id + '/status/';
        },

        isActive: function() {
            var status = this.get('ibsStatusId');
            return _.indexOf(['wosCANCELLED', 'wosCANCELLED_DONE', 'wosDONE', 'wosLOADED'], status) == -1;
        },

        isCompleted: function() {
            return this.get('ibsStatusId') === 'wosDONE';
        },

        canSendReceipt: function() {
            return this.isCompleted() && !!this.get('fareAvailable');
        }
    });


}());