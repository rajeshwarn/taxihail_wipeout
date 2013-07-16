(function () {

    var View = TaxiHail.ManagePaymentSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        events: {
            'change [name=paymentMode]': 'onPaymentModeChanged',
            'change [name=acceptChange]': 'onAcceptPaymentModeChange',
            'click #testConfigButton': 'testConfig',
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
                submitHandler: this.save,
            });

            return this;
        },

        testConfig: function ()
        {
            var data = this.$el.serializeObject();

            this.model.test(data)
                .done(_.bind(function(response) {
                    if (response.isSuccessful === true) {
                        this.alert(response.message, 'success');
                    } else {
                        this.alert(response.message, 'error');
                    }
                }, this));

        },
        
        alert: function (message, type) {
            
            message = replaceAll("\n", "<br/>", message);
            
            var alert = new TaxiHail.AlertView({
                message: this.localize(message),
                type: type
                });
            alert.on('ok', alert.remove, alert);
            this.$('.message').html(alert.render().el);
            this.model.fetch();
        },
        
        save: function(form) {

            var data = $(form).serializeObject();
            this.$("#warning").hide();
            
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

    function replaceAll(find, replace, str) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

}());