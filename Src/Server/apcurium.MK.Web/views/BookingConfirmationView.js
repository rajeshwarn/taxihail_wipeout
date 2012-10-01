(function () {
    var settings;
    var settingschanged = false;
    
    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=cancel]': 'cancel',
            'change :text[data-action=changepickup]': 'onPickupPropertyChanged',
            'change :text[data-action=changesettings]': 'onSettingsPropertyChanged',
            'change :input[data-action=changesettings]': 'onSettingsPropertyChanged'
        },
        initialize: function () { 

            _.bindAll(this, "renderResults");
            
            var pickup = this.model.get('pickupAddress');
            var dest = this.model.get('dropOffAddress');
            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup['latitude'], pickup['longitude'], dest['latitude'], dest['longitude']).done(this.renderResults);
            }
            

            this.referenceData = new TaxiHail.ReferenceData();
            this.referenceData.fetch();
            this.referenceData.on('change', this.render, this);

        },

        render: function (param) {

            this.$el.html(this.renderTemplate(this.model.toJSON()));
            //this.renderItem(this.model);
            

            Handlebars.registerHelper('ifCond', function (v1, v2, options) {
                if (v1 == v2) {
                    return options.fn(this);
                } else {
                    return options.inverse(this);
                }
            });

            var data = this.model.toJSON();

            _.extend(data, {
                vehiclesList: this.referenceData.attributes.vehiclesList,
                paymentsList: this.referenceData.attributes.paymentsList
            });

            this.$el.html(this.renderTemplate(data));

            if (this.model.get('dropOffAddress')) {
                this.showInfos(TaxiHail.localize('Warning_when_booking_without_destination'));
            }

            return this;
        },
        
        renderResults: function (result) {
            
            this.model.set({
                'priceEstimate': result.formattedPrice,
                'distanceEstimate': result.formattedDistance
            });
            this.render();
        },
        
        book: function (e) {
            this.$('#bookBt').button('loading');
            e.preventDefault();
            //this.model.set('settings', settings);
            this.model.save({}, {
                success : TaxiHail.postpone(function (model) {
                    // Wait for order to be created before redirecting to status
                        TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                }, this),
                error: this.showErrors
            });
            
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.destroyLocal();
            TaxiHail.app.navigate('', { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
        },
        
        showErrors: function (model, result) {
            this.$('#bookBt').button('reset');
            
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }
            var $alert = $('<div class="alert alert-error" />');
            if (result.statusText) {
                $alert.append($('<div />').text(this.localize(result.statusText)));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.errorCode)));
            }, this);
            this.$('.errors').html($alert);
        },
        
        showInfos : function (message) {
            var infos = $('<div class="alert alert-block" />').text(message);

            
            this.$('.infos').html(infos);
        },
        
        onPickupPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            var pickup = this.model.get('pickupAddress');
            
            pickup[$input.attr("name")] = $input.val();
            settingschanged = true;
        },
        
        onSettingsPropertyChanged : function (e) {
            var $input = $(e.currentTarget);
            var pickup = this.model.get('settings');

            pickup[$input.attr("name")] = $input.val();
            settingschanged = true;
        }
        
    });

}());


