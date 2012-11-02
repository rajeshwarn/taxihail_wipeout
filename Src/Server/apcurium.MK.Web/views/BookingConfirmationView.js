(function () {
    var settings,
        popoverTemplate = '<div class="popover popover-warning"><div class="arrow"></div><div class="popover-inner"><div class="popover-content"><p></p></div></div></div>';
    
    var View = TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=cancel]': 'cancel',
            'change :input': 'onPropertyChanged'
        },
        initialize: function () {

            _.bindAll(this, 'book', "renderResults", 'showErrors');
            
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
            

            this.validate({
                rules: {
                    'settings.name': "required",
                    'settings.phone': {
                        required: true,
                        regex: /^\(?([0-9]{3})\)?[\-. ]?([0-9]{3})[\-. ]?([0-9]{4})$/
                    },
                    'settings.passengers': {
                        required: true,
                        number: true
                    }
                },
                messages: {
                    'settings.name': {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    'settings.phone': {
                        required: TaxiHail.localize('error.PhoneRequired'),
                        regex: TaxiHail.localize('error.PhoneBadFormat')
                    },
                    'settings.passengers': {
                        required: TaxiHail.localize('error.PassengersRequired'),
                        number: TaxiHail.localize('error.NotANumber')
                    }
                },
                submitHandler: this.book
            });

            return this;
        },
        
        renderResults: function (result) {
            if (result.callForPrice) {
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
        
        book: function (form) {
            
            this.model.save({}, {
                success : TaxiHail.postpone(function (model) {
                    // Wait for order to be created before redirecting to status
                        TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                }, this),
                    error: this.showErrors
                });
        },
        
        cancel: function (e) {
            e.preventDefault();
            this.model.destroyLocal();
            TaxiHail.app.navigate('', { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
        },
        
        showErrors: function (model, result) {
            this.$(':submit').button('reset');
            
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }
            var $alert = $('<div class="alert alert-error" />');
            if (result.statusText) {
                $alert.append($('<div />').text(this.localize(result.statusText)));
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
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget),
                attr = $input.attr('name').split('.');

            if(attr.length > 1 && this.model.has(attr[0])) {
                this.model.get(attr[0])[attr[1]] = $input.val();
            } else {
                this.model.set(attr[0], $input.val());
            }
        }
        
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());


