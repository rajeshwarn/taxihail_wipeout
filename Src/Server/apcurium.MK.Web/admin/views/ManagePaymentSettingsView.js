(function () {

    var view = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [id=isSandbox]': 'onPayPalSettingsChanged',
            'change [id=sandboxClientId]': 'onPayPalSettingsChanged',
            'change [id=sandboxClientSecret]': 'onPayPalSettingsChanged',
            'change [id=prodClientId]': 'onPayPalSettingsChanged',
            'change [id=prodClientSecret]': 'onPayPalSettingsChanged',
            'change [name=isChargeAccountPaymentEnabled]': 'canPaymentMethodBeMandatory',
            'change [name=isPaymentOutOfAppDisabled]': 'canPaymentMethodBeMandatory',
            'change [name=isPayInTaxiEnabled]': 'canPaymentMethodBeMandatory',
            'change [name=paymentMode]': 'canPaymentMethodBeMandatory',
            'change [name=acceptChange]': 'onAcceptSettingsChanged',
            'change [name=acceptPayPalChange]': 'onAcceptSettingsChanged',
            'change [name=acceptChangeChargeAccount]': 'onAcceptSettingsChanged',
            'click #brainTreeSettingsButton': 'brainTreeSettingsButtonClick',
            'click #cmtSettingsButton': 'cmtSettingsButtonClick',
            'click #monerisSettingsButton': 'monerisSettingsButtonClick'
        },


        saveButton: {},
        warningDiv: {},
        payPalWarningDiv: {},
        updatedModel: {},
        chargeAccountDiv: {},

        render: function() {

            var data = this.model.toJSON();
            this.updatedModel = data.serverPaymentSettings;

            this.$el.html(this.renderTemplate(data.serverPaymentSettings));

            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode + "]").attr("selected", "selected");
            this.$("[id=landingPageType] option[value=" + data.serverPaymentSettings.payPalServerSettings.landingPageType + "]").attr("selected", "selected");

            this.warningDiv = this.$("#warning");
            this.payPalWarningDiv = this.$("#payPalWarning");
            this.saveButton = this.$("#saveButton");

            this.chargeAccountDiv = this.$("#warningChargeAccount");

            this.onPaymentModeChanged();
            this.onPayPalSettingsChanged();

            this.onChargeAccountSettingsChanged();
            this.canPaymentMethodBeMandatory();

            this.validate({
                rules: {
                    preAuthAmount: {
                        required: true,
                        number: true,
                        min: 50
                    }
                },
                submitHandler: this.save
            });

            return this;
        },

        testConfig: function(serviceCall, messageZoneSelector) {

            serviceCall
                .fail(_.bind(function(response) {
                    this.alert(response.status + ' - ' + response.statusText, 'error');
                }, this))
                .done(_.bind(function(response) {
                    if (response.isSuccessful === true) {
                        this.alert(response.message, 'success', messageZoneSelector);
                    } else {
                        this.alert(response.message, 'error', messageZoneSelector);
                    }
                }, this));
        },

        cmtSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testCmt(data), this.$("#cmtSettingsMessageZone"));
        },

        brainTreeSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testBraintree(data), this.$("#brainTreeSettingsMessageZone"));
        },

        monerisSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testMoneris(data), this.$("#monerisSettingsMessageZone"));
        },

        payPalProductionSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testPayPalProduction(data), this.$("#payPalProductionSettingsMessageZone"));
        },

        testPayPalSandboxSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testPayPalSandbox(data), this.$("#payPalSandboxSettingsMessageZone"));
        },

        alert: function(message, type, selector) {

            if (!selector) {
                selector = this.$('.message');
            }

            message = replaceAll("\n", "<br/>", message);

            var alert = new TaxiHail.AlertView({
                message: this.localize(message),
                type: type
            });
            alert.on('ok', alert.remove, alert);
            selector.html(alert.render().el);
            this.model.fetch();
        },

        save: function(form) {

            var data = $(form).serializeObject();

            this.$("#warning").hide();
            this.$('#warningChargeAccount').hide();

            if (data.paymentMode == "None" && data.isPayInTaxiEnabled == "true") {
                this.alert("Please select a payment method or disable Card on File Payment");

                this.$(':submit').button('reset');
                return;
            }

            if (data.isPayInTaxiEnabled != "true"
                && data.isChargeAccountPaymentEnabled != "true"
                && data.isPaymentOutOfAppDisabled != "None") {
                this.alert("Please select a payment method or enable In Car Payment");

                this.$(':submit').button('reset');
                return;
            }

            if (data.paymentMode == "None" || data.paymentMode == "RideLinqCmt") {
                data.automaticPaymentPairing = false;
            }

            this.model.save(data)
                .always(_.bind(function() {
                    this.$(':submit').button('reset');
                }, this))
                .done(_.bind(function() {
                    this.updatedModel = data;
                    this.alert('Settings Saved', 'success');

                }, this))
                .fail(_.bind(function() {

                    this.alert('Error Saving Settings', 'error');

                }, this));
        },

        validateWarning: function(warning) {
            return warning.is(":visible") && warning.prop("checked") || !warning.is(":visible");
        },

        onAcceptSettingsChanged: function() {
            var chargeAccountWarning = this.$("[name = acceptChangeChargeAccount]");
            var paymentWarning = this.$("[name = acceptChange]");

            if (this.validateWarning(chargeAccountWarning) && this.validateWarning(paymentWarning)) {
                this.saveButton.removeAttr('disabled');
            } else {
                this.saveButton.attr('disabled', 'disabled');
            }
        },

        onChargeAccountSettingsChanged: function () {
            var isChargeAccountPaymentEnabled = this.updatedModel.isChargeAccountPaymentEnabled;

            var newIsChargeAccountPaymentEnabled = this.$("[name = isChargeAccountPaymentEnabled]").val() == 'true';

            if (!newIsChargeAccountPaymentEnabled && newIsChargeAccountPaymentEnabled != isChargeAccountPaymentEnabled) {
                this.$("[name=acceptChangeChargeAccount]").removeAttr("checked");
                this.saveButton.attr('disabled', 'disabled');
                this.chargeAccountDiv.show();
            } else {
                this.chargeAccountDiv.hide();
            }

            this.onAcceptSettingsChanged();
        },

        canPaymentMethodBeMandatory: function (event) {

            if (event) {

                if (event.target.name == 'isChargeAccountPaymentEnabled') {
                    this.onChargeAccountSettingsChanged();
                }

                if (event.target.name == 'paymentMode') {
                    this.onPaymentModeChanged();
                }
            }

            var newIsChargeAccountPaymentEnabled = this.$("[name = isChargeAccountPaymentEnabled]").val() == 'true';
            var newIsPaymentOutOfAppDisabled = this.$("[name = isPaymentOutOfAppDisabled]").val() != 'None';
            var newIsPayInTaxiEnabled = this.$("[name = isPayInTaxiEnabled]").val() == 'true';
            var newPaymentMode = this.$("[name = paymentMode]").val();

            var inputCreditCardMandatory = this.$("[name=creditCardIsMandatory]");

            if ((!newIsChargeAccountPaymentEnabled && !newIsPayInTaxiEnabled && newIsPaymentOutOfAppDisabled) || newPaymentMode == 'None') {
                inputCreditCardMandatory.val('false');
                inputCreditCardMandatory.attr('disabled', 'disabled');
            } else {
                inputCreditCardMandatory.val(this.updatedModel.creditCardIsMandatory.toString());
                inputCreditCardMandatory.removeAttr('disabled');
            }
        },

        onPaymentModeChanged: function () {
            
            this.$("[name = acceptChange]").removeAttr("checked");

            var newPaymentMode = this.$("[name=paymentMode]").val();

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");
            var monerisDiv = this.$("#monerisSettingsDiv");
            var cmtRideLinqDiv = this.$("#cmtRideLinqDiv");

            var preAuthAmountEnabledDiv = this.$("#preAuthAmountEnabledDiv");
            var preAuthAmountDiv = this.$("#preAuthAmountDiv");
            var isUnpairingDisabledDiv = this.$("#isUnpairingDisabledDiv");
            var unpairingTimeOutDiv = this.$("#unpairingTimeOutDiv");
            var cancelOrderOnUnpairDiv = this.$("#cancelOrderOnUnpairDiv");

            var currentPaymentMode = this.updatedModel.paymentMode;

            if (newPaymentMode != currentPaymentMode) {
                if ((newPaymentMode == "Cmt" && currentPaymentMode == "RideLinqCmt") || (newPaymentMode == "RideLinqCmt" && currentPaymentMode == "Cmt")) {
                    this.warningDiv.hide();
                } else {
                    this.warningDiv.show();
                }
            } else {
                this.warningDiv.hide();
            }
            this.onAcceptSettingsChanged();

            if (newPaymentMode == "Cmt" || newPaymentMode == "RideLinqCmt")
            {
                btDiv.hide();
                cmtDiv.show();
                if (newPaymentMode == "RideLinqCmt") {
                    cmtRideLinqDiv.show();
                } else {
                    cmtRideLinqDiv.hide();
                }
                monerisDiv.hide();
            }
            else if (newPaymentMode == "Braintree")
            {
                btDiv.show();
                cmtDiv.hide();
                monerisDiv.hide();
            }
            else if (newPaymentMode == "Moneris")
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.show();
            }
            else
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.hide();
            }

            if (newPaymentMode == 'None') {
                preAuthAmountEnabledDiv.hide();
                preAuthAmountDiv.hide();
                isUnpairingDisabledDiv.hide();
                unpairingTimeOutDiv.hide();
                cancelOrderOnUnpairDiv.hide();
            } else {
                preAuthAmountEnabledDiv.show();
                preAuthAmountDiv.show();
                isUnpairingDisabledDiv.show();
                unpairingTimeOutDiv.show();
                cancelOrderOnUnpairDiv.show();
            }
        }
    });

    _.extend(view.prototype, TaxiHail.ValidatedView);

    function replaceAll(find, replace, str) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

}());