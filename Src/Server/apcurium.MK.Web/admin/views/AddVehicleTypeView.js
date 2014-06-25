(function () {
    
    var View = TaxiHail.AddVehicleTypeView = TaxiHail.TemplatedView.extend({

        className: 'add-vehicletype-view',

        events: {
            'click [data-action=destroy]': 'destroyVehicleType',
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
                    referenceDataVehicleId: "required",
                    logoName: "required",
                    name: "required"
                },
                messages: {
                    referenceDataVehicleId: {
                        required: TaxiHail.localize('error.vehicleTypeRefIdRequired')
                    },
                    logoName: {
                        required: TaxiHail.localize('error.vehicleTypeLogoNameRequired')
                    },
                    name: {
                        required: TaxiHail.localize('error.vehicleTypeNameRequired')
                    },
                },
                submitHandler: this.save
            });
 
            return this;
        },
        
        save: function (form) {
            var vehicleType = this.serializeForm(form);
            var vehicleType = _.extend(this.model.toJSON(), vehicleType);
            this.model.save(vehicleType, {
                success: _.bind(function(model){

                    this.collection.add(model);
                    TaxiHail.app.navigate('vehicleTypes', { trigger: true });

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
        
        destroyVehicleType: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Remove Vehicle Type'),
                message: this.localize('modal.removeVehicleType.message')
            }).on('ok', function () {
                this.model.destroy({ url: TaxiHail.parameters.apiRoot + '/admin/vehicletypes/' + this.model.get('id') });
                TaxiHail.app.navigate('vehicleTypes', { trigger: true });
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