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

            var data = _.extend(this.model.toJSON(), {
                availableVehicles: this.options.availableVehicles.toJSON(),
                networkVehicleTypes: this.options.networkVehicleTypes.toJSON(),
                serviceTypes: TaxiHail.ServiceTypesEnum(),
                isNetworkEnabled: TaxiHail.parameters.isNetworkEnabled,
                isNew: this.model.isNew()
            });

            var html = this.renderTemplate(data);
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

            vehicleType.isWheelchairAccessible = $("[name=isWheelchairAccessible]")[0].checked;

            var isValid = this.validateNetworkVehiclePassengerNumbers(vehicleType);
            if (!isValid) {
                this.$(':submit').button('reset');

                var alert = new TaxiHail.AlertView({
                    message: TaxiHail.localize(TaxiHail.localize('error.invalidNetworkPassengerNumber')),
                    type: 'error'
                });
                alert.on('ok', alert.remove, alert);
                this.$('.errors').html(alert.render().el);

                return;
            }

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
        
        validateNetworkVehiclePassengerNumbers: function(vehicleType) {
            var networkVehicles = this.options.networkVehicleTypes.toJSON();
            var selectedNetworkVehicle = null;

            for (var i = 0; i < networkVehicles.length; i++) {
                var networkVehicle = networkVehicles[i];

                if (vehicleType.referenceNetworkVehicleTypeId == networkVehicle.id) {
                    selectedNetworkVehicle = networkVehicle;
                    break;
                }
            }

            if (selectedNetworkVehicle && selectedNetworkVehicle.maxNumberPassengers == 0 && vehicleType.maxNumberPassengers != 0) {
                // Only a 'no limit' vehicle can be matched with another 'no limit' vehicle
                return false;
            }

            if (selectedNetworkVehicle && vehicleType.maxNumberPassengers < selectedNetworkVehicle.maxNumberPassengers) {
                return false;
            }

            return true;
        },

        destroyVehicleType: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('modal.removeVehicleType.title'),
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