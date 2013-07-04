(function () {

    var View = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        render: function () {
            
            var data = this.model.toJSON();
            
            this.$el.html(this.renderTemplate(data.serverPaymentSettings ));
            
            this.validate({
                submitHandler: this.save
            });

            return this;
        },

        save: function(form) {

            var data = this.serializeForm(form);

            var newData = {};
            newData = {};
            newData.companyId = this.model.toJSON().serverPaymentSettings.companyId;
            newData.paymentMode = data.paymentMode;

            newData.braintreeClientSettings = {};
            newData.braintreeClientSettings.clientKey = data.braintreeClientKey;

            newData.braintreeServerSettings = {};
            newData.braintreeServerSettings.isSandBox = data.braintreeIsSandBox;
            newData.braintreeServerSettings.merchantId = data.braintreeMerchantId;
            newData.braintreeServerSettings.privateKey = data.braintreePrivateKey;
            newData.braintreeServerSettings.publicKey = data.braintreePublicKey;
            
            newData.cmtPaymentSettings = {};
            newData.cmtPaymentSettings.baseUrl = data.cmtBaseUrl;
            newData.cmtPaymentSettings.consumerSecretKey = data.cmtConsumerSecretKey;
            newData.cmtPaymentSettings.customerKey = data.cmtConsumerSecretKey;
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
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());