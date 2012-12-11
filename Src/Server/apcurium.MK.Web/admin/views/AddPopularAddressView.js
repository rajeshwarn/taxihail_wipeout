(function () {

    var View = TaxiHail.AddPopularAddressView = TaxiHail.TemplatedView.extend({

        className: 'add-favorite-view',

        events: {
            'click [data-action=destroy]': 'destroyAddress',
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function () {
            _.bindAll(this, 'save');
        },

        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);

            this.validate({
                rules: {
                    friendlyName: "required",
                    fullAddress: "required",
                    streetNumber: "required",
                    street: "required",
                    city: "required",
                    zipCode: "required",
                    state: "required",
                    latitude: {
                        required: true,
                        number: true
                    },
                    longitude: {
                        required: true,
                        number: true
                    }
                },
                messages: {
                    friendlyName: {
                        required: TaxiHail.localize('error.friendlyNameRequired')
                    },
                    streetNumber: {
                        required: TaxiHail.localize('error.streetNumberRequired')
                    },
                    street: {
                        required: TaxiHail.localize('error.streetRequired')
                    },
                    city: {
                        required: TaxiHail.localize('error.cityRequired')
                    },
                    zipCode: {
                        required: TaxiHail.localize('error.zipCodeRequired')
                    },
                    state: {
                        required: TaxiHail.localize('error.stateRequired')
                    },
                    fullAddress: {
                        required: TaxiHail.localize('error.fullAddressRequired')
                    },
                    latitude: {
                        required: TaxiHail.localize('error.latitudeRequired'),
                        number: TaxiHail.localize('error.latitudeNumber')
                    },
                    longitude: {
                        required: TaxiHail.localize('error.longitudeRequired'),
                        number: TaxiHail.localize('error.longitudeNumber')
                    }
                },
                submitHandler: this.save
            });

            return this;
        },

        save: function (form) {
            var address = this.serializeForm(form);
            var address = _.extend(this.model.toJSON(), address);
            address.fullAddress = address.streetNumber + " " + address.street + ", " + address.city + ", " + address.state + " " + address.zipCode;
            
            this.model.save(address, {
                success: _.bind(function(model){

                    this.collection.add(model);
                    TaxiHail.app.navigate('addresses/popular', {trigger: true});

                }, this)
            });
        },

        destroyAddress: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Remove Popular Address'),
                message: this.localize('modal.removePopular.message')
            }).on('ok', function () {
                this.model.destroy();
                TaxiHail.app.navigate('addresses/popular', {trigger: true});
            }, this);
        },

        remove: function () {
            if(this._selector) this._selector.remove();
            $(document).off('click', this.ondocumentclick);
            this.$el.remove();
            return this;
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());