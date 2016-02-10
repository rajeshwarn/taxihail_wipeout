(function () {

    var view = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [id=isPayPalEnabled]': 'onPayPalSettingsChanged',
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
            'click #testPayPalSandboxSettingsButton': 'testPayPalSandboxSettingsButtonClick',
            'click #payPalProductionSettingsButton': 'payPalProductionSettingsButtonClick',
            'click #brainTreeSettingsButton': 'brainTreeSettingsButtonClick',
            'click #cmtSettingsButton': 'cmtSettingsButtonClick',
            'click #cmtSettingsLuxuryButton': 'cmtSettingsLuxuryButtonClick',
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
            this.$("[name=isPaymentOutOfAppDisabled] option[value=" + data.serverPaymentSettings.isPaymentOutOfAppDisabled + "]").attr("selected", "selected");
            this.$("[id=cmtPaymentSettings_pairingMethod] option[value=" + data.serverPaymentSettings.cmtPaymentSettings.pairingMethod + "]").attr("selected", "selected");
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
            this.testConfig(this.model.testCmt(data, 'Taxi'), this.$("#cmtSettingsMessageZone"));
        },

        cmtSettingsLuxuryButtonClick: function () {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testCmt(data, 'Luxury'), this.$("#cmtSettingsMessageZoneLuxury"));
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
            this.$("#payPalWarning").hide();
            this.$('#warningChargeAccount').hide();

            if (data.paymentMode == "None" && data.isPayInTaxiEnabled == "true") {
                this.alert("Please select a payment method or disable Card on File Payment");

                this.$(':submit').button('reset');
                return;
            }

            if (data.isPayInTaxiEnabled != "true"
                && data.isChargeAccountPaymentEnabled != "true"
                && data.isPaymentOutOfAppDisabled != "None"
                && data.payPalClientSettings.isEnabled != "true") {
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
            var payPalWarning = this.$("[name = acceptPayPalChange]");

            if (this.validateWarning(chargeAccountWarning) && this.validateWarning(paymentWarning) && this.validateWarning(payPalWarning)) {
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
        
        onPayPalSettingsChanged: function() {
            var currentPaymentSettings = this.updatedModel;
            
            this.$("[name = acceptPayPalChange]").removeAttr("checked");

            var paymentMode = this.$("[name=paymentMode]").val();
            var preAuthAmountEnabledDiv = this.$("#preAuthAmountEnabledDiv");
            var preAuthAmountDiv = this.$("#preAuthAmountDiv");
            var isUnpairingDisabledDiv = this.$("#isUnpairingDisabledDiv");
            var unpairingTimeOutDiv = this.$("#unpairingTimeOutDiv");
            var cancelOrderOnUnpairDiv = this.$("#cancelOrderOnUnpairDiv");

            var newIsPayPalEnabled = this.$("[id=isPayPalEnabled]").val() == 'true';
            var newIsSandboxValue = this.$("[id=isSandbox]").val() == 'true';
            var newSandboxClientId = this.$("[id=sandboxClientId]").val();
            var newSandboxClientSecret = this.$("[id=sandboxClientSecret]").val();
            var newProdClientId = this.$("[id=prodClientId]").val();
            var newProdClientSecret = this.$("[id=prodClientSecret]").val();

            // currentPaymentSettings.payPalClientSettings.isSandbox can sometimes be a string instead of a boolean.
            var oldIsSandbox = currentPaymentSettings.payPalClientSettings.isSandbox == true 
                ? currentPaymentSettings.payPalClientSettings.isSandbox
                : currentPaymentSettings.payPalClientSettings.isSandbox == 'true';

            var environmentChanged = newIsSandboxValue != oldIsSandbox;

            var sandboxSettingsChanged = newSandboxClientId != currentPaymentSettings.payPalClientSettings.sandboxCredentials.clientId
                || newSandboxClientSecret != currentPaymentSettings.payPalServerSettings.sandboxCredentials.secret;

            var prodSettingsChanged = newProdClientId != currentPaymentSettings.payPalClientSettings.credentials.clientId
                || newProdClientSecret != currentPaymentSettings.payPalServerSettings.credentials.secret;

            // Show hide unlink warning
            if (environmentChanged || sandboxSettingsChanged || prodSettingsChanged) {
                this.payPalWarningDiv.show();
            } else {
                this.payPalWarningDiv.hide();
            }
            this.onAcceptSettingsChanged();

            // Show/ hide preauth fields
            if (!newIsPayPalEnabled && paymentMode == 'None') {
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
        },

        onPaymentModeChanged: function () {
            
            this.$("[name = acceptChange]").removeAttr("checked");

            var newPaymentMode = this.$("[name=paymentMode]").val();

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");
            var monerisDiv = this.$("#monerisSettingsDiv");
            var cmtRideLinqDiv = this.$("#cmtRideLinqDiv");

            var isPayPalEnabled = this.$("[id=isPayPalEnabled]").val() == 'true';
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

            if (!isPayPalEnabled && newPaymentMode == 'None') {
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