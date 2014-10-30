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

        showEta: function() {
            return this.get('ibsStatusId') === 'wosASSIGNED' && this.hasVehicle() && TaxiHail.parameters.isEtaEnabled;
        },

        canSendReceipt: function() {
            return this.isCompleted() && !!this.get('fareAvailable');
        },

        hasVehicle: function () {
            // Check if we have all required information to display the vehicle position on a map
            // It's possible to receive coordinates but no vehicle number
            // if the driver has been assigned but has not yet accepted the offer.
            return this.get('vehicleLatitude') && this.get('vehicleLongitude') && this.get('vehicleNumber');
        }
    });


}());