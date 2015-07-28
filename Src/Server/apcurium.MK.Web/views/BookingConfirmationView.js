(function () {
    var settings,
        popoverTemplate = '<div class="popover popover-warning"><div class="arrow"></div><div class="popover-inner"><div class="popover-content"><p></p></div></div></div>';
    
    var View = TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=cancel]': 'cancel',
            'change :input': 'onPropertyChanged',
            "change #countrycode": "onPropertyChanged"
    },
        initialize: function () {
            _.bindAll(this, 'book', "renderResults", 'showErrors');

            var pickup = this.model.get('pickupAddress');
            var dest = this.model.get('dropOffAddress');
            var pickupZipCode = pickup.zipCode != null ? pickup.zipCode : '';
            var dropOffZipCode = (dest != null && dest.zipCode != null) ? dest.zipCode : '';

            this.showEstimate = TaxiHail.parameters.isEstimateEnabled && pickup && dest;
            this.showEstimateWarning = TaxiHail.parameters.isEstimateWarningEnabled;
            
            var accountNumber = this.model.get('accountNumber');

            if (this.showEstimate) {
                TaxiHail.directionInfo.getInfo(pickup.latitude,
                    pickup.longitude,
                    dest.latitude,
                    dest.longitude,
                    pickupZipCode,
                    dropOffZipCode,
                    this.model.get('settings')['vehicleTypeId'],
                    this.model.get('pickupDate'),
                    accountNumber
                    ).done(this.renderResults);
            }

            this.model.validateOrder(false)
                .done(_.bind(function (result) {

                    this.hasWarning = result.hasWarning;
                    this.message = result.message;
                    this.render();

                }, this));

            $.validator.addMethod("regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }
            );
            $.validator.addMethod("tenOrMoreDigits",
                function (value, element) {
                    var match = value.match(/\d/g);
                    if (match == null) return false;
                    var count = match.length;
                    return count >= 10;
                }
            );

            this.render();
        },

        render: function (param) {

            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-popover]').popover('hide');

            var data = this.model.toJSON();

            var chargeTypes = [];
            var refDataChargeTypes = TaxiHail.referenceData.paymentsList;

            if (this.model.get('market')) {
                // PayInCar and CoF are the only charge type when in external market

                for (var i = 0; i < refDataChargeTypes.length; i++) {
                    if (refDataChargeTypes[i].id === 1) {
                        // Pay in Car
                        chargeTypes.push(refDataChargeTypes[i]);
                    } else if (refDataChargeTypes[i].id === 3) {
                        // Card on File
                        chargeTypes.push(refDataChargeTypes[i]);
                    }
                }
            } else {
                // All available charge types
                chargeTypes = TaxiHail.referenceData.paymentsList;
            }

            // Remove CoF option since there's no card in the user profile
            if (TaxiHail.parameters.isBraintreePrepaidEnabled && !TaxiHail.auth.account.get('defaultCreditCard') && !TaxiHail.parameters.alwaysDisplayCoFOption) {
                var chargeTypesClone = chargeTypes.slice();
                for (var i = 0; i < chargeTypesClone.length; i++) {
                    var chargeType = chargeTypesClone[i];
                    if (chargeType.id == 3) {
                        chargeTypesClone.splice(i, 1);
                        chargeTypes = chargeTypesClone;
                    }
                }
            }

            // Validates that the paymentsList contains the currently set chargeTypeId (in booking settings). If not, use the first item in the list.
            var chargeTypeIdFound = false;
            var currentlySelectedSettingChargeTypeId = this.model.get('settings')['chargeTypeId'];
            for (var i = 0; i < chargeTypes.length; i++) {
                if (chargeTypes[i].id == currentlySelectedSettingChargeTypeId) {
                    chargeTypeIdFound = true;
                    break;
                }
            }

            if (!chargeTypeIdFound) {
                var chargeTypeId = -1;

                for (var i = 0; i < chargeTypes.length; i++) {
                    // We will ignore the Charge Account type.
                    if (!this.model.isChargeAccount(chargeTypes[i].id)) {
                        chargeTypeId = chargeTypes[i].id;
                        break;
                    }
                }

                if (chargeTypeId == -1 && chargeTypes.length > 0) {
                    chargeTypeId = chargeTypes[0].id;
                }

                this.model.get('settings')['chargeTypeId'] = chargeTypeId;
                data.settings.chargeTypeId = chargeTypeId;
            }

            _.extend(data, {
                vehiclesList: TaxiHail.vehicleTypes,
                paymentsList: chargeTypes,
                showPassengerNumber: TaxiHail.parameters.showPassengerNumber,
                showEstimate: this.showEstimate,
                countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes)
            });

            this.$el.html(this.renderTemplate(data));
            
            this.$("#countrycode").val(data.settings.country.code).selected = "true";

            if (!this.model.has('dropOffAddress')) {
                this.$('[data-dropoff]').text(TaxiHail.localize('NotSpecified'));
            }
            
            if ( (this.showEstimate) && ( this.showEstimateWarning )) {
                this.showEstimatedFareWarning();
            }
            

            if (this.hasWarning) {
                var $alert = $('<div class="alert alert-info" />');
                   $alert.append($('<div />').text(this.message));                
                this.$('.errors').html($alert);
            }

            this.validate({
                rules: {
                    'settings.name': "required",
                    'settings.phone': {
                        required: true
                    },
                    'settings.passengers': {
                        required: true,
                        number: true
                    },
                    'settings.largeBags': {
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
                    },
                    'settings.largeBags': {
                        number: TaxiHail.localize('error.NotANumber')
                    }
                },
                submitHandler: this.book
            });

            return this;
        },
        
        renderResults: function (result) {
            this.model.set({ 'estimate': result });
            if (result.callForPrice) {
                this.model.set('estimateDisplay', TaxiHail.localize("CallForPrice"));
            } else
            if (result.noFareEstimate) {
                this.model.set('estimateDisplay', TaxiHail.localize("NoFareEstimate"));
            } else
            if (result.formattedPrice || result.formattedDistance) {
                this.model.set('estimateDisplay', TaxiHail.localize('Estimate Display').format(result.formattedPrice, "(" + result.formattedDistance + ")"));
            } else  {
                this.model.set('estimateDisplay', TaxiHail.localize("NoFareEstimate"));
            }
            this.render();
        },

        remove: function() {

            this.$('[data-popover]').popover('hide');
            this.$el.remove();
        },
        
        book: function (form) {
            var lang = TaxiHail.getClientLanguage();
            this.model.set('ClientLanguageCode', lang);
            this.model.set('FromWebApp', true);
            this.model.saveLocal();

            this.$('.errors').html('');        

            var numberOfPassengers = this.model.get('settings')['passengers'];
            var vehicleType;
            var vehicleTypeId = this.model.get('settings')['vehicleTypeId'];
            if (typeof vehicleTypeId !== 'undefined') {
                // Try to match vehicle type to the prefered type in user profile
                vehicleType = $.grep(TaxiHail.vehicleTypes, function (e) { return e.referenceDataVehicleId == vehicleTypeId; })[0];
                if (!vehicleType) {
                    // If no match is found, use the first vehicle type
                    vehicleType = TaxiHail.vehicleTypes[0];
                    this.model.get('settings')['vehicleTypeId'] = vehicleType.referenceDataVehicleId;
                }
            }

            if (TaxiHail.parameters.showPassengerNumber
                && vehicleType.maxNumberPassengers > 0
                && numberOfPassengers > vehicleType.maxNumberPassengers) {
                this.$(':submit').button('reset');

                var $alert = $('<div class="alert alert-error" />');
                $alert.append($('<div />').text(TaxiHail.localize("CreateOrder_InvalidPassengersNumber")));
                this.$('.errors').html($alert);
                return;
            }

            if (this.model.get('settings')["chargeTypeId"] < 0) {
                this.$(':submit').button('reset');

                var $alert = $('<div class="alert alert-error" />');
                $alert.append($('<div />').text(TaxiHail.localize("CreateOrder_InvalidChargeType")));
                this.$('.errors').html($alert);
                return;
            }

            var hasCreditCardSet = TaxiHail.auth.account.get('defaultCreditCard') != null;

            if (this.model.isPayingWithAccountCharge() && !this.model.get('market')) {
                //account charge type payment                
                TaxiHail.app.navigate('bookaccountcharge', { trigger: true });

            } else if (TaxiHail.parameters.alwaysDisplayCoFOption
                && !hasCreditCardSet
                && this.model.isPayingWithCoF()
                && !this.model.get('market')) {

                if (!this.model.has('dropOffAddress')) {
                    this.$(':submit').button('reset');

                    var $alert = $('<div class="alert alert-error" />');
                    $alert.append($('<div />').text(TaxiHail.localize("CreateOrder_PrepaidNoEstimate")));
                    this.$('.errors').html($alert);
                } else {
                    TaxiHail.app.navigate('confirmationbook/payment', { trigger: true });
                }
                
            }
            else if (TaxiHail.parameters.askForCVVAtBooking
                && hasCreditCardSet
                && this.model.isPayingWithCoF()
                && !this.model.get('market')) {
                // navigate to CVV screen
                TaxiHail.app.navigate('confirmcvv', { trigger: true });
            }
            else {
                this.model.save({}, {
                    success: TaxiHail.postpone(function (model) {
                        // Wait for response before doing anything
                        ga('send', 'event', 'button', 'click', 'book web', 0);
                        if (this.model.isPayingWithPayPal()) {
                            window.location.replace(model.get('payPalCheckoutUrl'));
                        } else {
                            TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                        }
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
            this.$(':submit').button('reset');
            
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }

            var $alert = $('<div class="alert alert-error" />');
            if (result.errorCode == "CreateOrder_PendingOrder") {
                $alert.append($('<div />').text(this.localize(result.errorCode)));
            }
            else if (result.errorCode) {
                $alert.append($('<div />').text(result.message));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.statusText)));
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

            var dataNodeName = e.currentTarget.nodeName.toLowerCase();
            var elementName = e.currentTarget.name;


            var $input = $(e.currentTarget),
                attr = $input.attr('name').split('.');
           
            if (dataNodeName == "input") {
                if (attr.length > 1 && this.model.has(attr[0])) {
                    this.model.get(attr[0])[attr[1]] = $input.val();

                    if ([attr[1]] == "vehicleTypeId" || [attr[1]] == "chargeTypeId") {
                        this.initialize();
                    }
                } else {
                    this.model.set(attr[0], $input.val());
                }
            }
            else if (dataNodeName == "select") {
                if (elementName == "countryCode") {
                    this.model.attributes.settings.country.code = $input.find(":selected").val();
                }
            }
        }
        
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());

