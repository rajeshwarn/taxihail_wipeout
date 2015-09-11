(function () {

    var View = TaxiHail.AddPopularAddressView = TaxiHail.TemplatedView.extend({

        className: 'add-favorite-view',

        events: {
            'click [data-action=destroy]': 'destroyAddress',
            'click [data-action=cancel]': 'cancel',
            'change [name=addressLocationType]': 'onAddressLocationTypeChanged',
        },

        initialize: function () {
            _.bindAll(this, 'save');
        },

        render: function () {
        	var data = this.model.toJSON();

			//Ensure that airportId is correctly displayed.
	        data.placeId = data.placeReference;
            var html = this.renderTemplate(data);
            this.$el.html(html);

            this.$("[name=addressLocationType] option[value=" + data.addressLocationType + "]").attr("selected", "selected");

            this.validate({
                rules: {
                    friendlyName: "required",
                    fullAddress: "required",
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

        onAddressLocationTypeChanged: function() {
        	var addressLocationType = this.$("[name = addressLocationType]");
        	var airportReferenceDiv = this.$("[name = airportReferenceDiv]");

        	if (addressLocationType.val() == "1") {
		        airportReferenceDiv.show();
        	} else {
        		airportReferenceDiv.hide();
	        }
        },

        save: function (form) {
            var address = this.serializeForm(form);
            
            this.model.save(address, {
                success: _.bind(function(model){

                    this.collection.add(model);
                    TaxiHail.app.navigate('addresses/popular', {trigger: true});

                }, this),
                error: this.showErrors
            });
        },

        showErrors: function (model, result) {
            this.$(':submit').button('reset');

            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }

            var $alert = $('<div class="alert alert-error" />');
            $alert.append($('<div />').text(result.message));

            this.$('.errors').html($alert);
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