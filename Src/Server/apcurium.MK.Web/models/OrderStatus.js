(function(){

    TaxiHail.OrderStatus = Backbone.Model.extend({

        initialize: function() {
            this.on('change:ibsStatusId', function(model, value, options) {
                if (value === 'wosTIMEOUT') {
                    model.trigger('ibs:timeout');
                }
            });
        },

        url: function() {
            return 'api/account/orders/' + this.id + '/status/';
        },

        isActive: function() {
            var status = this.get('ibsStatusId');
            return _.indexOf(['wosCANCELLED', 'wosCANCELLED_DONE', 'wosDONE', 'wosLOADED', 'wosTIMEOUT'], status) == -1;
        },

        isCompleted: function() {
            return this.get('ibsStatusId') === 'wosDONE';
        },

        canSendReceipt: function() {
            return this.isCompleted() && !!this.get('fareAvailable');
        }
    });


}());