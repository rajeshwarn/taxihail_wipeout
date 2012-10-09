(function () {
    var settings,
        settingschanged = false,
        popoverTemplate = '<div class="popover popover-warning"><div class="arrow"></div><div class="popover-inner"><div class="popover-content"><p></p></div></div></div>';
    
    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=cancel]': 'cancel',
            'change :text[data-action=changepickup]': 'onPickupPropertyChanged',
            'change :text[data-action=changesettings]': 'onSettingsPropertyChanged',
            'change :input[data-action=changesettings]': 'onSettingsPropertyChanged'
        },
        initialize: function () {

            _.bindAll(this, "renderResults", 'showErrors');
            
            var pickup = this.model.get('pickupAddress');
            var dest = this.model.get('dropOffAddress');
            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup['latitude'], pickup['longitude'], dest['latitude'], dest['longitude']).done(this.renderResults);
            }
            

            this.referenceData = new TaxiHail.ReferenceData();
            this.referenceData.fetch();
            this.referenceData.on('change', this.render, this);
            
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }

            );

        },

        render: function (param) {

            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-popover]').popover('hide');

            var data = this.model.toJSON();

            _.extend(data, {
                vehiclesList: this.referenceData.attributes.vehiclesList,
                paymentsList: this.referenceData.attributes.paymentsList
            });

            this.$el.html(this.renderTemplate(data));


            //this.$(':text, select').editInPlace();

            if (this.model.has('dropOffAddress')) {
                this.showEstimatedFareWarning();
            } else {
                this.$('[data-dropoff]').text(TaxiHail.localize('NotSpecified'));
            }
            

            this.$("#updateBookingSettingsForm").validate({
                rules: {
                    name: "required",
                    phone: {
                        required: true,
                        regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/
                    },
                    passengers: {
                        required: true,
                        number: true
                    }
                },
                messages: {
                    name: {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    phone: {
                        required: TaxiHail.localize('error.PhoneRequired'),
                        regex: TaxiHail.localize('error.PhoneBadFormat')
                    },
                    passengers: {
                        required: TaxiHail.localize('error.PassengersRequired'),
                        number: TaxiHail.localize('error.NotANumber')
                    }
                },
                highlight: function (label) {
                    $(label).closest('.control-group').addClass('error');
                    $(label).prevAll('.valid-input').addClass('hidden');
                }, success: function (label) {
                    $(label).closest('.control-group').removeClass('error');
                    label.prevAll('.valid-input').removeClass('hidden');

                }
            });

            return this;
        },
        
        renderResults: function (result) {
            if (result.price > 100) {
                this.model.set('priceEstimate', TaxiHail.localize("CallForPrice"));
            } else {
                this.model.set('priceEstimate', result.formattedPrice);
            }
            this.model.set({
                'distanceEstimate': result.formattedDistance
            });
            this.render();
        },

        remove: function() {

            this.$('[data-popover]').popover('hide');
            this.$el.remove();
        },
        
        book: function (e) {
            
            e.preventDefault();
            if (this.$("#updateBookingSettingsForm").valid()) {
                this.$('#bookBt').button('loading');
                this.model.save({}, {
                success : TaxiHail.postpone(function (model) {
                    // Wait for order to be created before redirecting to status
                        TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                }, this),
                error: this.showErrors
            });
            }
        },
        
        cancel: function (e) {
            e.preventDefault();
            this.model.destroyLocal();
            TaxiHail.app.navigate('', { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
        },
        
        showErrors: function (model, result) {
            this.$('#bookBt').button('reset');
            
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }
            var $alert = $('<div class="alert alert-error" />');
            if (result.statusText) {
                if (result.statusText == "CreateOrder_CannotCreateInIbs") {
                    $alert.append($('<div />').text(this.localize(result.statusText).prototype.format('','')));
                } else {
                    $alert.append($('<div />').text(this.localize(result.statusText)));
                }
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.errorCode)));
            }, this);
            this.$('.errors').html($alert);
        },
        
        showEstimatedFareWarning : function () {

            $('[data-popover]').popover({
                content: this.localize('EstimatedFareWarning'),
                trigger: 'manual',
                offsetX: -22,
                template: popoverTemplate
            }).popover('show');

        },
        
        onPickupPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            var pickup = this.model.get('pickupAddress');
            
            pickup[$input.attr("name")] = $input.val();
            settingschanged = true;
        },
        
        onSettingsPropertyChanged : function (e) {
            var $input = $(e.currentTarget);
            var pickup = this.model.get('settings');

            pickup[$input.attr("name")] = $input.val();
            settingschanged = true;
        }
        
    });

}());


