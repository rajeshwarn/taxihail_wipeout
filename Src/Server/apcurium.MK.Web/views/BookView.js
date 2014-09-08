(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({

        className: 'book-view',

        events: {
            'click [data-action=book]': 'book',
            'click [data-action=later]': 'later'
        },
        
        initialize: function () {
            this.model.on('change', function(model, value) {

                // Enable the buttons if model is valid
                if (this.model.isValidAddress('pickupAddress') && (!TaxiHail.parameters.isDestinationRequired || (  TaxiHail.parameters.isDestinationRequired && this.model.isValidAddress('dropOffAddress'))))
                {
                    this.$('.buttons .btn').prop('disabled', false).removeClass('disabled');
                } else this.$('.buttons .btn').prop('disabled', true).addClass('disabled');

            }, this);

           this.model.on('change:pickupAddress', function(model, value) {
                this._pickupAddressView.model.set(value);
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this._dropOffAddressView.model.set(value);
            }, this);

            // ===== Ride Estimate =====

            // Only show ride estimate if enabled
            TaxiHail.parameters.isEstimateEnabled &&
                this.model.on('change:pickupAddress change:dropOffAddress', function (model, value) {
                    this.actualizeEstimate();
                }, this);
            
            this.model.on('change:estimate', function (model, value) {
                var $estimate = this.$('.estimate');

                $estimate
                   .find('.distance')
                   .show();

                if (value.formattedPrice && value.formattedDistance) {
                    $estimate.removeClass('hidden')
                        .find('.distance')
                        .text('(' + value.formattedDistance + ')');
                     
                    if (value.callForPrice) {
                        $estimate
                            .find('.fare')
                            .text(TaxiHail.localize('CallForPrice'));
                        $estimate
                            .find('.label')
                            .hide();
                    }
                    else
                        if (value.noFareEstimate) {
                        $estimate
                            .find('.fare')
                            .text(TaxiHail.localize('NoFareEstimate'));
                        $estimate
                            .find('.label')
                            .hide();
                    }
                    else {
                        $estimate
                            .find('.fare')
                            .text(value.formattedPrice);
                        $estimate
                            .find('.label')
                            .show();
                    }
                    
                } else {
                    this.$('.estimate').addClass('hidden');
                }
             }, this);
            
           
        },
        
        

        render: function () {
            
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            var pickupAddress = new Backbone.Model(),
                dropOffAddress = new Backbone.Model();

            this._pickupAddressView = new TaxiHail.AddressControlView({
                    model: pickupAddress,
                    locate: true,
                    pin: 'green',
                    locatepopular : true
                });
            this._dropOffAddressView = new TaxiHail.AddressControlView({
                    model: dropOffAddress,
                    clear: true,
                    pin: 'red',
                    locatepopular : true
            });
            
            if (TaxiHail.parameters.disableFutureBooking) {
                this.$('#bookLaterButton').addClass('hidden');
            }

            this.$('.pickup-address-container').html(this._pickupAddressView.render().el);
            this.$('.drop-off-address-container').html(this._dropOffAddressView.render().el);
            
            
            this.model.set('isPickupActive', false);
            this.model.set('isDropOffActive', false);
            // Only one address picker can be open at once
           

            this._pickupAddressView.on('open', function(view){
                this._dropOffAddressView.close();
            }, this);

            this._dropOffAddressView.on('open', function(view){
                this._pickupAddressView.close();
            }, this);

            this._pickupAddressView.on('target', function (view, isActive) {
                this.model.set({
                    'isPickupActive': isActive,
                    'isDropOffActive': false
                });
                this._dropOffAddressView.toggleOff();
            }, this);

            this._dropOffAddressView.on('target', function (view, isActive) {
                this.model.set({
                    'isDropOffActive': isActive,
                    'isPickupActive': false
                });
                this._pickupAddressView.toggleOff();
            }, this);

            pickupAddress.on('change', function(model){
                this.model.set({
                    pickupAddress: model.toJSON()
                });
            }, this);

            dropOffAddress.on('change', function(model){
                this.model.set({
                    dropOffAddress: model.toJSON()
                });
            }, this);

            if (!this.model.isValidAddress('pickupAddress') || (TaxiHail.parameters.isDestinationRequired && !this.model.isValidAddress('dropOffAddress'))) {
                this.$('.buttons .btn').addClass('disabled');
            }
            return this;
        },

        remove: function() {
            if(this._pickupAddressView) this._pickupAddressView.remove();
            if(this._dropOffAddressView) this._dropOffAddressView.remove();
            this.$el.remove();
            return this;
        },
        
        actualizeEstimate: function () {

            var $estimate = this.$('.estimate');
            $estimate
                .find('.fare')
                .text(TaxiHail.localize('Loading'));
            $estimate
                .find('.distance')
                .hide();

            var pickup = this.model.get('pickupAddress'),
                dest = this.model.get('dropOffAddress');

            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup.latitude, pickup.longitude, dest.latitude, dest.longitude)
                    .done(_.bind(function(result){

                        this.model.set({ 'estimate': result });

                    }, this));
            }
           
        },
        
        book: function (e) {
            e.preventDefault();
            if(this.model.isValidAddress('pickupAddress')) {
                this.model.saveLocal();
                TaxiHail.app.navigate('confirmationbook', { trigger:true });
            }
        },

        later: function (e) {
            e.preventDefault();
            if(this.model.isValidAddress('pickupAddress')) {
                this.model.saveLocal();
                TaxiHail.app.navigate('later', { trigger:true });
            }
        }
    });

}());