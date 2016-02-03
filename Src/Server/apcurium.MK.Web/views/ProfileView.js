(function () {
    
    var View = TaxiHail.ProfileView = TaxiHail.TemplatedView.extend({
        events: {
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

            _.bindAll(this, 'savechanges');

            $.validator.addMethod(
                "regex",
                function(value, element, regexp) {
                    var re = new RegExp(regexp);
                    return this.optional(element) || re.test(value);
                }
            );

            this.render();
        },

        render: function () {
            var data = this.model.toJSON();

            if (!data.settings.accountNumber) {
                // We need to make sure that the account number, if empty, is saved as a string
                data.settings.accountNumber = "";
            }
            if (!data.settings.customerNumber) {
                // We need to make sure that the customer number, if empty, is saved as a string
                data.settings.customerNumber = "";
            }
            if (!data.settings.payBack) {
                // We need to make sure that the PayBack number, if empty, is saved as a string
                data.settings.payBack = "";
            }

            if (!data.settings.country.code) {
                // We need to make sure that the PayBack number, if empty, is saved as a string
                data.settings.country.code = TaxiHail.parameters.defaultCountryCode;
            }

            var showPayBackField = false;
            var isPayBackFieldRequired = false;
            if (TaxiHail.parameters.isPayBackRegistrationFieldRequired != null) {
                showPayBackField = true;
                isPayBackFieldRequired = TaxiHail.parameters.isPayBackRegistrationFieldRequired == "true";
            }

            var chargeTypes = TaxiHail.referenceData.paymentsList;

            // Remove CoF option since there's no card in the user profile
            if ((TaxiHail.parameters.isBraintreePrepaidEnabled || TaxiHail.parameters.isCMEnabled || TaxiHail.parameters.isRideLinqCMTEnabled)
                && !TaxiHail.auth.account.get('defaultCreditCard')) {
                var chargeTypesClone = chargeTypes.slice();
                for (var i = 0; i < chargeTypesClone.length; i++) {
                    var chargeType = chargeTypesClone[i];
                    if (chargeType.id == 3) {
                        chargeTypesClone.splice(i, 1);
                        chargeTypes = chargeTypesClone;
                    }
                }
            }

            _.extend(data, {
                vehiclesList: TaxiHail.vehicleTypes,
                paymentsList: chargeTypes,
                isChargeAccountPaymentEnabled: TaxiHail.parameters.isChargeAccountPaymentEnabled,
                showPayBackField: showPayBackField,
                countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes)
            });

            this.$el.html(this.renderTemplate(data));

            this.$("#countrycode").val(data.settings.country.code).selected = "true";
            
            this.validate({
                rules: {
                    name: "required",
                    phone: {
                        required : true
                    },
                    passengers: {
                        required: true,
                        number : true
                    },
                    payBack: {
                        required: isPayBackFieldRequired,
                        regex: /^\d{0,10}$/  // Up to 10 digits
                    }
                },
                messages: {
                    name: {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    phone: {
                        required: TaxiHail.localize('error.PhoneRequired')
                    },
                    passengers: {
                        required: TaxiHail.localize('error.PassengersRequired'),
                        number: TaxiHail.localize('error.NotANumber')
                    },
                    payBack: {
                        required: TaxiHail.localize('error.PayBackRequired'),
                        regex: TaxiHail.localize('error.PayBackBadFormat')
                    }
                },
                submitHandler: this.savechanges
            });

            return this;
        },

        renderConfirmationMessage: function() {
            var view = new TaxiHail.AlertView({
                message: this.localize('Changes were saved'),
                type: 'success'
            });
            view.on('ok', this.render, this);
            this.$el.html(view.render().el);
        },
        
        savechanges: function (form) {
            var accountNumber = this.model.get('settings').accountNumber;
            var customerNumber = this.model.get('settings').customerNumber;
            var chargeAccountEnabled = TaxiHail.parameters.isChargeAccountPaymentEnabled;

            if (chargeAccountEnabled && accountNumber) {

                // Validate charge account number
                this.model.getChargeAccount(accountNumber, customerNumber)
                    .then(_.bind(function() {
                        this.updateSettings();
                    }, this))
                    .fail(_.bind(function () {
                            this.$(':submit').button('reset');

                            var alert = new TaxiHail.AlertView({
                                message: TaxiHail.localize("Account Not Found"),
                                type: 'error'
                            });
                            alert.on('ok', alert.remove, alert);
                            this.$('.errors').html(alert.render().el);
                    }, this));
            } else {
                // No charge account number to validate
                this.updateSettings();
            }
        },
        
        updateSettings: function() {
            // Update settings
            this.model.get("settings").email = this.model.get("email");
            this.model.updateSettings()
                .done(_.bind(function() {
                    this.renderConfirmationMessage();
                }, this))
                .fail(_.bind(function (result) {
                    this.$(':submit').button('reset');

                    var message = "";

                    if (result.statusText != undefined) {
                        message = result.statusText;
                    }
                    else {
                        message = TaxiHail.localize("error.accountUpdate");
                    }

                    var alert = new TaxiHail.AlertView({
                        message: message,
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);
                }, this));
        },

        onPropertyChanged: function (e) {

            var elementName = e.currentTarget.name;

            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            if (elementName == "countryCode") {
                settings.country.code = $input.find(":selected").val();
            } else {
                var name = $input.attr("name");
                var value = $input.val();
                settings[name] = value;
            }


            this.$(':submit').removeClass('disabled');
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());
