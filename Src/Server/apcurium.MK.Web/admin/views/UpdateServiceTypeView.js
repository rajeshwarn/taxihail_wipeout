(function () {
    
    var View = TaxiHail.UpdateServiceTypeView = TaxiHail.TemplatedView.extend({

        className: 'update-servicetype-view',

        events: {
            'click [data-action=cancel]': 'cancel'
        },
        
        initialize :function () {
            _.bindAll(this, 'save');
        },
        
        render: function () {

            var data = _.extend(this.model.toJSON(), {
                isNew: this.model.isNew()
            });
            var html = this.renderTemplate(data);
            this.$el.html(html);

            this.validate({
                rules: {
                    serviceType: "required",
                    ibsWebServicesUrl: "required",
                    waitTimeRatePerMinute: "required",
                    airportMeetAndGreetRate: "required"
                },
                messages: {
                    ibsWebServicesUrl: {
                        required: TaxiHail.localize('error.serviceTypeIbsWebServicesUrlRequired')
                    },
                    waitTimeRatePerMinute: {
                        required: TaxiHail.localize('error.serviceTypeWaitTimeRatePerMinuteRequired')
                    },
                    airportMeetAndGreetRate: {
                        required: TaxiHail.localize('error.serviceTypeAirportMeetAndGreetRateRequired')
                    }
                },
                submitHandler: this.save
            });
 
            return this;
        },
        
        save: function (form) {
            var serviceTypeSettings = this.serializeForm(form);
            var serviceTypeSettings = _.extend(this.model.toJSON(), serviceTypeSettings);

            this.model.save(serviceTypeSettings, {
                success: _.bind(function(model){
                    this.collection.add(model);
                    TaxiHail.app.navigate('serviceTypes', { trigger: true });

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

        cancel : function (e) {
            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());