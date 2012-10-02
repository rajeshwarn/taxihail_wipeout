(function () {
    var settingschanged = false;
    TaxiHail.ProfileView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=savechanges]': 'savechanges',
            'change :text': 'onPropertyChanged',
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

            this.referenceData = new TaxiHail.ReferenceData();
            this.referenceData.fetch();
            this.referenceData.on('change', this.render, this);

        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            
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

            
            this.$("#updateSettingsForm").validate({
                rules: {
                    name: "required",
                    phone: "required",
                    passengers: "required",
                },
                messages: {
                    name: {
                        required: TaxiHail.localize('error.NameRequired'),
                    },
                    phone: {
                        required: TaxiHail.localize('error.PhoneRequired'),
                    },
                    passengers: {
                        required: TaxiHail.localize('error.PassengersRequired'),
                    }
                }, success: function (label) {
                }
            });

            return this;
        },
        
        savechanges : function (e) {
            e.preventDefault();
            var settings = this.model.get('settings');
            if (this.$("#updateSettingsForm").valid() ) {
                            
  
                                    
                                    jQuery.ajax({
                                                type: 'PUT',
                                                url: 'api/account/bookingsettings',
                                                data: settings,
                                                success: function () {
                                                    $("#notif-bar").html(TaxiHail.localize('Settings Changed'));
                                                    
                                                    
                                                  },
                                                  error: this.showErrors,
                                                  dataType: 'json'
                                    });
                        }
               
                
        },
        
        onPropertyChanged : function (e) {
            e.preventDefault();
            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            settings[$input.attr("name")] = $input.val();
            settingschanged = true;
            this.$('[data-action=savechanges]').removeClass('disabled');
        }
    });

}());
