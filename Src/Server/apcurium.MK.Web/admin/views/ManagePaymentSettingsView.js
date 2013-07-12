(function () {

    var View = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [name=paymentMode]': 'onPaymentModeChanged',
            'change [name=acceptChange]': 'onAcceptPaymentModeChange',
        },
        
        saveButton: {},
        warningDiv:{},
        
        render: function () {
            
            var data = this.model.toJSON();
            
            this.$el.html(this.renderTemplate(data.serverPaymentSettings));
            
            this.$("[name=paymentMode] option[value=" + data.serverPaymentSettings.paymentMode +"]").attr("selected","selected") ;

            this.warningDiv = this.$("#warning");
            this.saveButton = this.$("#saveButton");
            
            this.onPaymentModeChanged();

            this.validate({
                submitHandler: this.save
            });

            return this;
        },

        save: function(form) {

            var data = $(form).serializeObject();
            this.$("#warning").hide();


            this.model.batchSave(data)
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
                     this.model.fetch();

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
        
        onAcceptPaymentModeChange: function () {
            
            if (this.$("[name = acceptChange]").prop("checked")) {
                this.saveButton.removeAttr('disabled');
            } else {
                this.saveButton.attr('disabled', 'disabled');
            }
        },
                
        onPaymentModeChanged: function () {
            
            this.$("[name = acceptChange]").removeAttr("checked");

            var method = this.$("[name=paymentMode]").val();

            var btDiv = this.$("#braintreeSettingsDiv");
            var cmtDiv = this.$("#cmtSettingsDiv");


            if (this.model.toJSON().serverPaymentSettings.paymentMode != method) {
                this.warningDiv.show();
                this.onAcceptPaymentModeChange();
            } else {
                this.warningDiv.hide();

                this.saveButton.removeAttr('disabled');
            }

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