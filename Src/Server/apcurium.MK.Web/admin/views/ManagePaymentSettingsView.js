(function () {

    var view = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [id=isPayPalEnabled]': 'onPayPalSettingsChanged',
            'change [id=isSandbox]': 'onPayPalSettingsChanged',
            'change [id=SandboxClientId]': 'onPayPalSettingsChanged',
            'change [id=SandboxClientSecret]': 'onPayPalSettingsChanged',
            'change [id=ProdClientId]': 'onPayPalSettingsChanged',
            'change [id=ProdClientSecret]': 'onPayPalSettingsChanged',

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

        render: function () {

            var data = this.model.toJSON();

            this.$el.html(this.renderTemplate(data.serverPaymentSettings));
            
            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode + "]").attr("selected", "selected");

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
            
            if (data.paymentMode == "None" && data.isPayInTaxiEnabled == 'true') {
                this.alert("Please select a payment method or disable Pay In Taxi");

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

                     this.alert('Settings Saved', 'success');

                 }, this))
                 .fail(_.bind(function(){

                     this.alert('Error Saving Settings', 'error');
                     
                 }, this));
        },
        
        onAcceptPaymentModeChange: function () {
            
            if (this.$("[name = acceptChange]").prop("checked")) {
                this.saveButton.removeAttr('disabled');
            } else {
                this.saveButton.attr('disabled', 'disabled');
            }
        },

        onAcceptPayPalSettingsChange: function () {

            if (this.$("[name = acceptPayPalChange]").prop("checked")) {
                this.saveButton.removeAttr('disabled');
            } else {
                this.saveButton.attr('disabled', 'disabled');
            }
        },
        
        onPayPalSettingsChanged: function() {
            var currentPaymentSettings = this.model.toJSON().serverPaymentSettings;

            var paymentMode = this.$("[name=paymentMode]").val();
            var preAuthAmountEnabledDiv = this.$("#preAuthAmountEnabledDiv");
            var preAuthAmountDiv = this.$("#preAuthAmountDiv");

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
                this.payPalWarningDiv.show();
                this.onAcceptPayPalSettingsChange();
            } else {
                this.payPalWarningDiv.hide();
                if (!this.warningDiv.is(':visible')) {
                    this.saveButton.removeAttr('disabled');
                }
            }

            // Show/ hide preauth fields
            if (!newIsPayPalEnabled && (paymentMode == 'None' || paymentMode == 'RideLinqCmt')) {
                preAuthAmountEnabledDiv.hide();
                preAuthAmountDiv.hide();
            } else {
                preAuthAmountEnabledDiv.show();
                preAuthAmountDiv.show();
            }
        },

        onPaymentModeChanged: function () {
            
            this.$("[name = acceptChange]").removeAttr("checked");

            var newPaymentMode = this.$("[name=paymentMode]").val();
            var autPairingDiv = this.$("#automaticPairingDiv");
            var noShowFeeDiv = this.$("#noShowFeeDiv");

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");
            var monerisDiv = this.$("#monerisSettingsDiv");

            var isPayPalEnabled = this.$("[id=isPayPalEnabled]").val();
            var preAuthAmountEnabledDiv = this.$("#preAuthAmountEnabledDiv");
            var preAuthAmountDiv = this.$("#preAuthAmountDiv");

            var method = this.model.toJSON().serverPaymentSettings.paymentMode;

            if (newPaymentMode != method) {
                if ((newPaymentMode == "Cmt" && method == "RideLinqCmt") || (newPaymentMode == "RideLinqCmt" && method == "Cmt")) {
                    this.warningDiv.hide();
                    if (!this.payPalWarningDiv.is(':visible')) {
                        this.saveButton.removeAttr('disabled');
                    }
                } else {
                    this.warningDiv.show();
                    this.onAcceptPaymentModeChange();
                }
            } else {
                this.warningDiv.hide();
                if (!this.payPalWarningDiv.is(':visible')) {
                    this.saveButton.removeAttr('disabled');
                }
            }

            if (newPaymentMode == "RideLinqCmt")
            {
                btDiv.hide();
                cmtDiv.show();
                monerisDiv.hide();
                autPairingDiv.hide();
            }
            else if (newPaymentMode == "Cmt")
            {
                btDiv.hide();
                cmtDiv.show();
                monerisDiv.hide();
                autPairingDiv.show();
                noShowFeeDiv.show();
            }
            else if (newPaymentMode == "Braintree")
            {
                btDiv.show();
                cmtDiv.hide();
                monerisDiv.hide();
                autPairingDiv.show();
                noShowFeeDiv.show();
            }
            else if (newPaymentMode == "Moneris")
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.show();
                autPairingDiv.show();
                noShowFeeDiv.show();
            }
            else
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.hide();
                autPairingDiv.hide();
                noShowFeeDiv.hide();
            }

            if ((!isPayPalEnabled && !newPaymentMode)
                || (isPayPalEnabled != "true" && (newPaymentMode == 'None' || newPaymentMode == 'RideLinqCmt'))) {
                preAuthAmountEnabledDiv.hide();
                preAuthAmountDiv.hide();
            } else {
                preAuthAmountEnabledDiv.show();
                preAuthAmountDiv.show();
            }
        }
    });

    _.extend(view.prototype, TaxiHail.ValidatedView);

    function replaceAll(find, replace, str) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

}());