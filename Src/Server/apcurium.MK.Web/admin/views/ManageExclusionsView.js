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
                submitHandler: this.save
            });

            return this;
        },

        save: function(form) {

            var data = this.serializeForm(form);


            var excludedVehicleTypeSetting = this.options.settings.get('IBS.ExcludedVehicleTypeId');
            var excludedPaymentTypeSetting = this.options.settings.get('IBS.ExcludedPaymentTypeId');
            var excludedProviderSetting = this.options.settings.get('IBS.ExcludedProviderId');
            excludedVehicleTypeSetting.set({
                value: _([data.vehiclesList]).flatten().join(';')
            });
            excludedPaymentTypeSetting.set({
                value: _([data.paymentsList]).flatten().join(';')
            });
            excludedProviderSetting.set({
                value: _([data.companiesList]).flatten().join(';')
            });


            $.when(excludedVehicleTypeSetting.save(), excludedPaymentTypeSetting.save(), excludedProviderSetting.save())
                .then(_.bind(function(){
                    this.$(':submit').button('reset');
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
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());