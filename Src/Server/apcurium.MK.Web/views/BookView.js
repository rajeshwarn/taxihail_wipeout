(function () {

    TaxiHail.BookView = TaxiHail.TemplatedView.extend({

        className: 'book-view',

        events: {
            'click [data-action=book]': 'book',
            'click [data-action=later]': 'later'
        },
        
        initialize: function () {

           this.model.on('change:pickupAddress', function(model, value) {
                this._pickupAddressView.model.set(value);
            }, this);

            this.model.on('change:dropOffAddress', function(model, value) {
                this._dropOffAddressView.model.set(value);
            }, this);

            // ===== Ride Estimate & ETA =====

            // Validate addresses + Only update ride estimate & eta if enabled
                this.model.on('change:pickupAddress change:dropOffAddress', function (model, value) {
                    this.validateOrderAndRefreshEstimate();
                }, this);
            
            TaxiHail.parameters.isEtaEnabled &&
                this.model.on('change:pickupAddress', function (model, value) {
                    this.actualizeEta();
                }, this);

            // Update UI values when server call is completed
            this.model.on('change:estimate', function (model, value) {
                    this.updateFareEstimateVisibility(value);
            }, this);

            this.model.on('change:eta', function (model, value) {
                    this.updateEtaDisplayVisibility(value);
            }, this);
        },
        
        updateFareEstimateVisibility: function (value) {

            var $estimate = this.$('.estimate');

            if (value.formattedPrice && value.formattedDistance) {
                $estimate.removeClass('hidden');
               

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
                            .text(TaxiHail.localize('Estimate Display').format(value.formattedPrice, "(" + value.formattedDistance + ")"));
                        $estimate
                            .find('.label')
                            .show();
                    }

            } else {
                this.$('.estimate').addClass('hidden');
            }
        },

        updateEtaDisplayVisibility: function (value) {
            var $eta = this.$('.eta');
            $eta
               .find('.etaValue')
               .show();

            if (value.etaDuration) {
                var formattedEta = TaxiHail.formatEta(value.etaDuration, value.etaFormattedDistance);

                $eta.removeClass('hidden')
                    .find('.etaValue')
                    .text(formattedEta);
            } else {
                this.$('.eta').addClass('hidden');
            }
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
                this.$('.buttons .btn').attr('disabled', 'disabled');
            }
            return this;
        },

        remove: function() {
            if(this._pickupAddressView) this._pickupAddressView.remove();
            if(this._dropOffAddressView) this._dropOffAddressView.remove();
            this.$el.remove();
            return this;
        },

        validateOrderAndRefreshEstimate: function()
        {
            var $estimate = this.$('.estimate');
            $estimate
                .find('.fare')
                .text(TaxiHail.localize('Loading'));

            if (TaxiHail.parameters.isEstimateEnabled
                && this.model.isValidAddress('pickupAddress')
                && this.model.isValidAddress('dropOffAddress')) {

                $estimate.removeClass('hidden');
            }

            this.$('.errors').html('');

            this.$('.buttons .btn').addClass('disabled');
            this.$('.buttons .btn').attr('disabled', 'disabled');

            this.model.validateOrder(true)
                    .done(_.bind(function (result) {

                        if (result.responseText) {
                            result = JSON.parse(result.responseText).responseStatus;
                        }

                        // Don't display validation errors if no destination address is specified when destination required is on
                        var destinationRequiredAndNoDropOff = TaxiHail.parameters.isDestinationRequired && !this.model.isValidAddress('dropOffAddress');

                        if (result.hasError && !destinationRequiredAndNoDropOff)
                        {
                            this.$('.buttons .btn').addClass('disabled');
                            this.$('.buttons .btn').attr('disabled', 'disabled');
                            this.showErrors(result);
                            $estimate
                                .addClass('hidden')
                                .find('.fare')
                                .text('--');
                            this.model.set({ 'estimate': '' });

                        } else {
                            if (!TaxiHail.parameters.isDestinationRequired
                                || (TaxiHail.parameters.isDestinationRequired && this.model.isValidAddress('dropOffAddress'))) {

                                this.$('.buttons .btn').removeClass('disabled');
                                this.$('.buttons .btn').removeAttr('disabled');
                            }

                            if (TaxiHail.parameters.isEstimateEnabled) {
                                this.actualizeEstimate();
                            }
                        }
                    }, this));
        },
        
        actualizeEstimate: function () {          

            var pickup = this.model.get('pickupAddress'),
                dest = this.model.get('dropOffAddress');

            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup.latitude, pickup.longitude, dest.latitude, dest.longitude)
                    .done(_.bind(function(result){

                        this.model.set({ 'estimate': result });

                    }, this));
            }
        },

        actualizeEta: function () {

            var pickup = this.model.get('pickupAddress');

            if (pickup) {
                TaxiHail.directionInfo.getEta(pickup.latitude, pickup.longitude)
                    .done(_.bind(function (result) {

                        this.model.set({ 'eta': result });

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
        },

        showErrors: function (validationResult) {         

            var $alert = '';
            if (validationResult.hasError)
            {
                var $alert = $('<div class="alert alert-error" />').text(validationResult.message);
            }            
            
            this.$('.errors').html($alert);
        }
    });

}());