(function () {

    var View = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [name=paymentMode]': 'onPaymentModeChanged'
        },

        
        render: function () {
            
            var data = this.model.toJSON();
            
            this.$el.html(this.renderTemplate(data.serverPaymentSettings));
            
            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode +"]").attr("selected","selected") ;

            this.onPaymentModeChanged();

            this.validate({
                submitHandler: this.save
            });

            return this;
        },

        save: function(form) {

            var data = this.serializeForm(form);

            var newData = this.model.toJSON();
            newData.companyId = serverPaymentSettings.companyId;
            newData.paymentMode = data.paymentMode;
            
            newData.braintreeClientSettings.clientKey = data.braintreeClientKey;

            newData.braintreeServerSettings.isSandbox = data.braintreeIsSandbox;
            newData.braintreeServerSettings.merchantId = data.braintreeMerchantId;
            newData.braintreeServerSettings.privateKey = data.braintreePrivateKey;
            newData.braintreeServerSettings.publicKey = data.braintreePublicKey;
            
            newData.cmtPaymentSettings.isSandbox = data.cmtIsSandbox;
            newData.cmtPaymentSettings.baseUrl = data.cmtBaseUrl;
            newData.cmtPaymentSettings.consumerSecretKey = data.cmtConsumerSecretKey;
            newData.cmtPaymentSettings.customerKey = data.cmtCustomerKey;
            newData.cmtPaymentSettings.merchantToken = data.cmtMerchantToken;
            newData.cmtPaymentSettings.sandboxBaseUrl = data.cmtSandboxBaseUrl;
            

            this.model.batchSave(newData)
                 .always(_.bind(function() {

                     this.$(':submit').button('reset');

                 }, this))
                 .done(_.bind(function(){

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Settings Saved'),
                         type: 'success'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this))
                 .fail(_.bind(function(){

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Error Saving Settings'),
                         type: 'error'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this));
        },
                
        onPaymentModeChanged: function() {
            var method = this.$("[name=paymentMode]").val();

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");

            if (method == "Cmt") {
                btDiv.hide();
                cmtDiv.show();
            }
            else if (method == "Braintree") {
                btDiv.show();
                cmtDiv.hide();
            } else {
                btDiv.hide();
                cmtDiv.hide();
            }
            

        },
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());