(function(){

    var View  = TaxiHail.ManageExclusionsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',

        render: function() {

            var data = this.model.toJSON();

            this._checkItems(data.vehiclesList, this.options.exclusions.get('excludedVehicleTypeId'));
            this._checkItems(data.companiesList, this.options.exclusions.get('excludedProviderId'));
            
            this.$el.html(this.renderTemplate(data));

            this.validate({
                submitHandler: this.save,
                rules: {
                    vehiclesList: { checkboxesNotAllChecked: { options: data.vehiclesList } },
                    companiesList: { checkboxesNotAllChecked: { options: data.companiesList } }
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

           this._save(data)
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

        _checkItems: function(items, excludedValues) {
            if (!(excludedValues && excludedValues.length)) {
                return items;
            }

            _.each(items, function(item) {
                item.checked = _.contains(excludedValues, item.id) ? 'checked' : '';
            }, this);
        },

        _save: function (data) {
            
            
            return this.options.exclusions.save({
                excludedVehicleTypeId: _([data.vehiclesList || '']).flatten().map(function(value){ return value; }),
                excludedProviderId: _([data.companiesList || '']).flatten().map(function(value){ return value; })
            });
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());