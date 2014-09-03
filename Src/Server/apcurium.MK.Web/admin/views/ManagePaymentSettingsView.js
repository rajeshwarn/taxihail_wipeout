(function () {

    var view = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [name=paymentMode]': 'onPaymentModeChanged',
            'change [name=acceptChange]': 'onAcceptPaymentModeChange',
            'click #testPayPalSandboxSettingsButton': 'testPayPalSandboxSettingsButtonClick',
            'click #payPalProductionSettingsButton': 'payPalProductionSettingsButtonClick',
            'click #brainTreeSettingsButton': 'brainTreeSettingsButtonClick',
            'click #cmtSettingsButton': 'cmtSettingsButtonClick',
            'click #monerisSettingsButton': 'monerisSettingsButtonClick'
        },


        saveButton: {},
        warningDiv: {},

        render: function () {

            var data = this.model.toJSON();

            this.$el.html(this.renderTemplate(data.serverPaymentSettings));
            
            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode + "]").attr("selected", "selected");

            this.warningDiv = this.$("#warning");
            this.saveButton = this.$("#saveButton");
            
            this.onPaymentModeChanged();

            this.validate({
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
            
            if (data.paymentMode == "None" && data.isPayInTaxiEnabled) {
                this.alert("Please select a payment method or disable Pay In Taxi");

                this.$(':submit').button('reset');
                return;
            }
            
            if (data.paymentMode == "None" || data.paymentMode == "RideLinqCmt") {
                data.automaticPayment = false;
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
                
        onPaymentModeChanged: function () {
            
            this.$("[name = acceptChange]").removeAttr("checked");

            var newPaymentMode = this.$("[name=paymentMode]").val();

            var autPaymentDiv = this.$("#automaticPaymentDiv");
            var autPairingDiv = this.$("#automaticPairingDiv");

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");
            var monerisDiv = this.$("#monerisSettingsDiv");

            var method = this.model.toJSON().serverPaymentSettings.paymentMode;

            if (newPaymentMode != method) {
                if ((newPaymentMode == "Cmt" && method == "RideLinqCmt") || (newPaymentMode == "RideLinqCmt" && method == "Cmt")) {
                    this.warningDiv.hide();
                    this.saveButton.removeAttr('disabled');
                } else {
                    this.warningDiv.show();
                    this.onAcceptPaymentModeChange();
                }
            } else {
                this.warningDiv.hide();
                this.saveButton.removeAttr('disabled');
            }

            if (newPaymentMode == "RideLinqCmt")
            {
                btDiv.hide();
                cmtDiv.show();
                monerisDiv.hide();
                autPaymentDiv.hide();
                autPairingDiv.hide();
            }
            else if (newPaymentMode == "Cmt")
            {
                btDiv.hide();
                cmtDiv.show();
                monerisDiv.hide();
                autPaymentDiv.show();
                autPairingDiv.show();
            }
            else if (newPaymentMode == "Braintree")
            {
                btDiv.show();
                cmtDiv.hide();
                monerisDiv.hide();
                autPaymentDiv.show();
                autPairingDiv.show();
            }
            else if (newPaymentMode == "Moneris")
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.show();
                autPaymentDiv.show();
                autPairingDiv.show();
            }
            else
            {
                btDiv.hide();
                cmtDiv.hide();
                monerisDiv.hide();
                autPaymentDiv.hide();
                autPairingDiv.hide();
            }
        }
    });

    _.extend(view.prototype, TaxiHail.ValidatedView);

    function replaceAll(find, replace, str) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

}());