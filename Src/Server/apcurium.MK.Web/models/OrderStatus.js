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

            if (typeof status == 'undefined' || status == null) {
                return false;
            }

            return _.indexOf(['wosCANCELLED', 'wosCANCELLED_DONE', 'wosDONE', 'wosLOADED', 'wosTIMEOUT'], status) == -1;
        },

        isCompleted: function() {
            return this.get('ibsStatusId') === 'wosDONE';
        },

        isWaitingToBeAssigned: function() {
            return this.get('ibsStatusId') === 'wosWAITING';
        },

        driverHasBailed: function () {
            return this.get('ibsStatusId') === 'wosBAILED';
        },

        showEta: function() {
            return this.get('ibsStatusId') === 'wosASSIGNED' && this.hasVehicle() && TaxiHail.parameters.isEtaEnabled;
        },

        warnForCancellationFees: function () {
            return TaxiHail.parameters.warnForFeesOnCancel
                && (this.get('ibsStatusId') === 'wosASSIGNED'
                    || this.get('ibsStatusId') === 'wosARRIVED');
        },

        canSendReceipt: function() {
            return this.isCompleted() && !!this.get('fareAvailable') && this.get('isPrepaid') === false;
        },

        hasVehicle: function () {
            // Check if we have all required information to display the vehicle position on a map
            // It's possible to receive coordinates but no vehicle number
            // if the driver has been assigned but has not yet accepted the offer.
            return this.get('vehicleLatitude') && this.get('vehicleLongitude') && this.get('vehicleNumber') && !this.driverHasBailed();
        }
    });


}());