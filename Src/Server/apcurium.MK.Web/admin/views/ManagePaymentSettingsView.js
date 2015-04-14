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

            'change [name=paymentMode]': 'onPaymentModeChanged',
            'change [name=acceptChange]': 'onAcceptPaymentModeChange',
            'change [name=acceptPayPalChange]': 'onAcceptPayPalSettingsChange',
            'click #testPayPalSandboxSettingsButton': 'testPayPalSandboxSettingsButtonClick',
            'click #payPalProductionSettingsButton': 'payPalProductionSettingsButtonClick',
            'click #brainTreeSettingsButton': 'brainTreeSettingsButtonClick',
            'click #cmtSettingsButton': 'cmtSettingsButtonClick',
            'click #monerisSettingsButton': 'monerisSettingsButtonClick'
        },


        saveButton: {},
        warningDiv: {},
        payPalWarningDiv: {},
        updatedModel: {},

        render: function () {

            var data = this.model.toJSON();
            this.updatedModel = data.serverPaymentSettings;

            this.$el.html(this.renderTemplate(data.serverPaymentSettings));
            
            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode + "]").attr("selected", "selected");
            this.$("[id=landingPageType] option[value=" + data.serverPaymentSettings.payPalServerSettings.landingPageType + "]").attr("selected", "selected");

            this.warningDiv = this.$("#warning");
            this.payPalWarningDiv = this.$("#payPalWarning");
            this.saveButton = this.$("#saveButton");
            
            this.onPaymentModeChanged();
            this.onPayPalSettingsChanged();

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

        testConfig: function (serviceCall, messageZoneSelector) {

            serviceCall
                .fail(_.bind(function(response) {
                    this.alert(response.status + ' - ' + response.statusText, 'error');
                }, this))
                .done(_.bind(function (response) {
                    if (response.isSuccessful === true) {
                        this.alert(response.message, 'success', messageZoneSelector);
                    } else {
                        this.alert(response.message, 'error', messageZoneSelector);
                    }
                }, this));
        },
        
        cmtSettingsButtonClick: function () {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testCmt(data), this.$("#cmtSettingsMessageZone"));
        },

        brainTreeSettingsButtonClick: function () {
            
            var data = this.$el.serializeObject();
            this.testConfig(this.model.testBraintree(data), this.$("#brainTreeSettingsMessageZone"));
        },

        monerisSettingsButtonClick: function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testMoneris(data), this.$("#monerisSettingsMessageZone"));
        },

        payPalProductionSettingsButtonClick: function () {
            
            var data = this.$el.serializeObject();
            this.testConfig(this.model.testPayPalProduction(data), this.$("#payPalProductionSettingsMessageZone"));
        },
        
        testPayPalSandboxSettingsButtonClick:function() {

            var data = this.$el.serializeObject();
            this.testConfig(this.model.testPayPalSandbox(data), this.$("#payPalSandboxSettingsMessageZone"));
        },
       
        alert: function (message, type, selector) {
            
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
            
            if (data.paymentMode == "None" && data.isPayInTaxiEnabled == "true") {
                this.alert("Please select a payment method or disable Card on File Payment");

                this.$(':submit').button('reset');
                return;
            }

            if (data.isPayInTaxiEnabled != "true" && data.isChargeAccountPaymentEnabled != "true" && data.isOutOfAppPaymentDisabled == "true") {
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
                 .fail(_.bind(function(){

                     this.alert('Error Saving Settings', 'error');
                     
                 }, this));
        },
        
        onAcceptPaymentModeChange: function () {

            var paymentWarning = this.$("[name = acceptChange]");
            var payPalWarning = this.$("[name = acceptPayPalChange]");

            if (payPalWarning.is(":visible")) {
                if (payPalWarning.prop("checked") && paymentWarning.prop("checked")) {
                    this.saveButton.removeAttr('disabled');
                }
            } else {
                if (paymentWarning.prop("checked")) {
                    this.saveButton.removeAttr('disabled');
                }
            }
        },

        onAcceptPayPalSettingsChange: function () {

            var paymentWarning = this.$("[name = acceptChange]");
            var payPalWarning = this.$("[name = acceptPayPalChange]");

            if (paymentWarning.is(":visible")) {
                if (paymentWarning.prop("checked") && payPalWarning.prop("checked")) {
                    this.saveButton.removeAttr('disabled');
                }
            } else {
                if (payPalWarning.prop("checked")) {
                    this.saveButton.removeAttr('disabled');
                }
            }
        },
        
        onPayPalSettingsChanged: function() {

            var currentPaymentSettings = this.updatedModel;
            
            this.$("[name = acceptPayPalChange]").removeAttr("checked");

            var paymentMode = this.$("[name=paymentMode]").val();
            var preAuthAmountEnabledDiv = this.$("#preAuthAmountEnabledDiv");
            var preAuthAmountDiv = this.$("#preAuthAmountDiv");
            var noShowFeeDiv = this.$("#noShowFeeDiv");
            var isUnpairingDisabledDiv = this.$("#isUnpairingDisabledDiv");
            var unpairingTimeOutDiv = this.$("#unpairingTimeOutDiv");

            var newIsPayPalEnabled = this.$("[id=isPayPalEnabled]").val() == 'true';
            var newIsSandboxValue = this.$("[id=isSandbox]").val() == 'true';
            var newSandboxClientId = this.$("[id=sandboxClientId]").val();
            var newSandboxClientSecret = this.$("[id=sandboxClientSecret]").val();
            var newProdClientId = this.$("[id=prodClientId]").val();
            var newProdClientSecret = this.$("[id=prodClientSecret]").val();

            var environmentChanged = newIsSandboxValue != currentPaymentSettings.payPalClientSettings.isSandbox;

            var sandboxSettingsChanged = newSandboxClientId != currentPaymentSettings.payPalClientSettings.sandboxCredentials.clientId
                || newSandboxClientSecret != currentPaymentSettings.payPalServerSettings.sandboxCredentials.secret;

            var prodSettingsChanged = newProdClientId != currentPaymentSettings.payPalClientSettings.credentials.clientId
                || newProdClientSecret != currentPaymentSettings.payPalServerSettings.credentials.secret;

            // Show hide unlink warning
            if (environmentChanged || sandboxSettingsChanged || prodSettingsChanged) {
                this.saveButton.attr('disabled', 'disabled');
                this.payPalWarningDiv.show();
                this.onAcceptPayPalSettingsChange();
            } else {
                this.payPalWarningDiv.hide();
            }

            // Show/ hide preauth fields
            if (!newIsPayPalEnabled && paymentMode == 'None') {
                preAuthAmountEnabledDiv.hide();
                preAuthAmountDiv.hide();
                noShowFeeDiv.hide();
                isUnpairingDisabledDiv.hide();
                unpairingTimeOutDiv.hide();
            } else {
                preAuthAmountEnabledDiv.show();
                preAuthAmountDiv.show();
                noShowFeeDiv.show();
                isUnpairingDisabledDiv.show();
                unpairingTimeOutDiv.show();
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
            var noShowFeeDiv = this.$("#noShowFeeDiv");

            var currentPaymentMode = this.updatedModel.paymentMode;

            if (newPaymentMode != currentPaymentMode) {
                if ((newPaymentMode == "Cmt" && currentPaymentMode == "RideLinqCmt") || (newPaymentMode == "RideLinqCmt" && currentPaymentMode == "Cmt")) {
                    this.warningDiv.hide();
                } else {
                    this.saveButton.attr('disabled', 'disabled');
                    this.warningDiv.show();
                    this.onAcceptPaymentModeChange();
                }
            } else {
                this.warningDiv.hide();
                if (!this.payPalWarningDiv.is(':visible')) {
                    this.saveButton.removeAttr('disabled');
                }
            }

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
                noShowFeeDiv.hide();
                isUnpairingDisabledDiv.hide();
                unpairingTimeOutDiv.hide();
            } else {
                preAuthAmountEnabledDiv.show();
                preAuthAmountDiv.show();
                isUnpairingDisabledDiv.show();
                unpairingTimeOutDiv.show();
                noShowFeeDiv.show();
            }
        }
    });

    _.extend(view.prototype, TaxiHail.ValidatedView);

    function replaceAll(find, replace, str) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

}());