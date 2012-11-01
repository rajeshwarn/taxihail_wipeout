(function(){

    var View  = TaxiHail.ManageExclusionsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',

        render: function() {

            var data = this.model.toJSON();

            this._checkItems(data.vehiclesList, 'IBS.ExcludedVehicleTypeId');
            this._checkItems(data.paymentsList, 'IBS.ExcludedPaymentTypeId');
            this._checkItems(data.companiesList, 'IBS.ExcludedProviderId');
            
            this.$el.html(this.renderTemplate(data));

            this.validate({
                submitHandler: this.save,
                rules: {
                    vehiclesList: { checkboxesNotAllChecked: { options: data.vehiclesList/*, optionsName: this.localize("Vehicle Types") */} },
                    paymentsList: { checkboxesNotAllChecked: { options: data.paymentsList/*, optionsName: this.localize("Payment Types") */} },
                    companiesList: { checkboxesNotAllChecked: { options: data.companiesList/*, optionsName: this.localize("Companies") */} }
                },
                errorPlacement: function (error, element) {
                    if (error.text() == "") {
                    } else {
                        var $form = element.closest("form");

                        var alert = new TaxiHail.AlertView({
                            message: error,
                            type: 'error'
                        });

                        alert.on('ok', alert.remove, alert);
                        $form.find('.message').html(alert.render().el);
                    }
                }
            });

            return this;
        },

        save: function(form) {

            var data = this.serializeForm(form);

            $.when(this._save(data.vehiclesList, 'IBS.ExcludedVehicleTypeId'),
                   this._save(data.paymentsList, 'IBS.ExcludedPaymentTypeId'),
                   this._save(data.companiesList, 'IBS.ExcludedProviderId'))
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

        _checkItems: function(items, settingKey) {
            var setting = this.options.settings.get(settingKey);
            if(!setting) {
                return items;
            }

            var checkedIds = (setting.get('value') || '').split(';');
            // Transform list into list of Numbers
            checkedIds = _.map(checkedIds, function(item){ return +item; });

            _.each(items, function(item) {
                item.checked = _.contains(checkedIds, item.id) ? 'checked' : '';
            }, this);
        },

        _save: function(items, settingKey) {
            var setting = this.options.settings.get(settingKey);
            if(!setting) {
                return null;
            }

            return setting.save({
                value: _([items]).flatten().join(';')
            });
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());