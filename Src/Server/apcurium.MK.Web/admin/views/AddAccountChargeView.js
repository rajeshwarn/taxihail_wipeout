(function () {
    
    var View = TaxiHail.AddAccountChargeView = TaxiHail.TemplatedView.extend({

        className: 'add-accountcharge-view',

        events: {
            'click [data-action=destroy]': 'destroyAccount',
            'click [data-action=cancel]': 'cancel'
        },
        
        initialize :function () {
            _.bindAll(this, 'save');
        },
        
        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);

            this.validate({
                rules: {
                    number: "required",
                    name: "required"
                },
                messages: {
                    number: {
                        required: TaxiHail.localize('error.accountNumberRequired')
                    },
                    name: {
                        required: TaxiHail.localize('error.accountNameRequired')
                    },
                },
                submitHandler: this.save
            });
 
            return this;
        },
        
        save: function (form) {
            var account = this.serializeForm(form);

            for (var i = 0; i < 8; i++) {
                var isRequiredCheckboxName = "questionIsRequiredFields" + i;
                var isCaseSensitiveCheckboxName = "questionIsCaseSensitiveFields" + i;

                var isRequiredCheckboxControl = $("[name=" + isRequiredCheckboxName + "]")[0];
                var isCaseSensitiveCheckboxControl = $("[name=" + isCaseSensitiveCheckboxName + "]")[0];

                if (!isRequiredCheckboxControl.checked) {
                    account[isRequiredCheckboxName] = "off";
                }

                if (!isCaseSensitiveCheckboxControl.checked) {
                    account[isCaseSensitiveCheckboxName] = "off";
                }
            }
            this.model.save(account, {
                success: _.bind(function(model){

                    this.collection.add(model);
                    TaxiHail.app.navigate('accounts', { trigger: true });

                }, this),
                error: function (model, xhr, options) {
                    this.$(':submit').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(xhr.statusText),
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);
                }
            });
        },
        
        destroyAccount: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Remove Account'),
                message: this.localize('modal.removeAccount.message')
            }).on('ok', function () {
                this.model.destroy({ url: TaxiHail.parameters.apiRoot + '/admin/accountscharge/' + this.model.get('number') });
                TaxiHail.app.navigate('accounts', { trigger: true });
            }, this);
        },

        remove: function() {
            this.$el.remove();
            return this;
        },

        cancel : function (e) {
            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());