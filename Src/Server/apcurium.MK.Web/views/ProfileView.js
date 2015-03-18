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

            var tipPercentages = [
                { id: 0, display: "0%" },
                { id: 5, display: "5%" },
                { id: 10, display: "10%" },
                { id: 15, display: "15%" },
                { id: 18, display: "18%" },
                { id: 20, display: "20%" },
                { id: 25, display: "25%" }
            ];

            var displayTipSelection = TaxiHail.parameters.isChargeAccountPaymentEnabled
                || TaxiHail.parameters.isPayPalEnabled;

            _.extend(data, {
                vehiclesList: TaxiHail.vehicleTypes,
                paymentsList: TaxiHail.referenceData.paymentsList,
                isChargeAccountPaymentEnabled: TaxiHail.parameters.isChargeAccountPaymentEnabled,
                displayTipSelection: displayTipSelection,
                tipPercentages: tipPercentages
            });

            this.$el.html(this.renderTemplate(data));
            
            this.validate({
                rules: {
                    name: "required",
                    phone: {
                        required : true,
                        regex: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/
                    },
                    passengers: {
                        required: true,
                        number : true
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
            if (accountNumber) { // || customerNumber

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
                .fail(_.bind(function() {
                    this.$(':submit').button('reset');
                }, this));
        },

        onPropertyChanged : function (e) {
            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            var name = $input.attr("name");
            var value = $input.val();

            // Update local model values
            if (name === "defaultTipPercent") {
                this.model.set("defaultTipPercent", value);
            }
            settings[name] = value;
            settings["defaultTipPercent"] = this.model.get("defaultTipPercent");

            this.$(':submit').removeClass('disabled');
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());
