﻿(function () {
    
    var View = TaxiHail.ProfileView = TaxiHail.TemplatedView.extend({
        events: {
            'change :input': 'onPropertyChanged',
            "change #countrycode": "onPropertyChanged"
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

            var tipPercentages = [
                { id: 0, display: "0%" },
                { id: 5, display: "5%" },
                { id: 10, display: "10%" },
                { id: 15, display: "15%" },
                { id: 18, display: "18%" },
                { id: 20, display: "20%" },
                { id: 25, display: "25%" }
            ];

            if (data.defaultTipPercent == null) {
                this.model.set('defaultTipPercent', TaxiHail.parameters.defaultTipPercentage);
            }

            var displayTipSelection = TaxiHail.parameters.isChargeAccountPaymentEnabled
                || TaxiHail.parameters.isPayPalEnabled
                || TaxiHail.parameters.isBraintreePrepaidEnabled;

            var showPayBackField = false;
            var isPayBackFieldRequired = false;
            if (TaxiHail.parameters.isPayBackRegistrationFieldRequired != null) {
                showPayBackField = true;
                isPayBackFieldRequired = TaxiHail.parameters.isPayBackRegistrationFieldRequired == "true";
            }

            var chargeTypes = TaxiHail.referenceData.paymentsList;

            // Remove CoF option since there's no card in the user profile
            if (TaxiHail.parameters.isBraintreePrepaidEnabled && !TaxiHail.auth.account.get('defaultCreditCard')) {
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
                displayTipSelection: displayTipSelection,
                tipPercentages: tipPercentages,
                showPayBackField: showPayBackField,
                countryCodes: TaxiHail.countryCodes
            });

            for (var i = 0; i < data.countryCodes.length; i++) {

                var spaces = [];
                var spacesToAdd = 2;

                if (data.countryCodes[i].CountryDialCode > 0) {
                    var sp = (3 - data.countryCodes[i].CountryDialCode.toString().length) * 2;
                    spacesToAdd += Math.max(sp, 0);
                }
                else {
                    spacesToAdd += 8;
                }

                for (var i1 = 0; i1 < spacesToAdd; i1++)
                    spaces.push(0);

                data.countryCodes[i].spaces = spaces;
            }

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

            var dataNodeName = e.currentTarget.nodeName.toLowerCase();
            var elementName = e.currentTarget.name;

            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            if (dataNodeName == "input") {
                var name = $input.attr("name");
                var value = $input.val();

                // Update local model values
                if (name === "defaultTipPercent") {
                    this.model.set("defaultTipPercent", value);
                }
                settings[name] = value;
                settings["defaultTipPercent"] = this.model.get("defaultTipPercent");
            }
            else if (dataNodeName == "select") {
                if (elementName == "countryCode") {
                    settings.country.code = $input.find(":selected").val();
                }
            }

            this.$(':submit').removeClass('disabled');
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());
