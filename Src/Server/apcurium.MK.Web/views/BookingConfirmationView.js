﻿(function () {
    var settings;
    var settingschanged = false;
    
    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=edit]': 'edit',
            'change :text': 'onPropertyChanged',
            
        },
        initialize: function () {   

            _.bindAll(this, "renderResults");
            
            var pickup = this.model.get('pickupAddress');
            var dest = this.model.get('dropOffAddress');
            if (pickup && dest) {
                TaxiHail.directionInfo.getInfo(pickup['latitude'], pickup['longitude'], dest['latitude'], dest['longitude']).done(this.renderResults);
            }
        },

        render: function (param) {

            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.$("input").attr("disabled", true);
            this.renderItem(this.model);
            return this;
        },
        
        renderResults: function (result) {
            
            this.model.set({
                'priceEstimate': result.formattedPrice,
                'distanceEstimate': result.formattedDistance
            });
        },
        
        book: function (e) {
            this.$('#bookBt').button('loading');
            e.preventDefault();
            this.model.set('settings', settings);
            this.model.save({},{success : function (value) {
                TaxiHail.app.navigate('bookconfirmed/' + value.get('iBSOrderId'), { trigger: true });
            }});
            
        },
        
        edit:function (e) {
            e.preventDefault();
            
            if (!$("input").attr("disabled")) {
                
                this.$('.errors').empty();
                if (settings.isValid() ) {
                    
                    if (settingschanged) {
                        
                        jQuery.ajax({
                                type: 'PUT',
                                url: 'api/account/bookingsettings',
                                data: settings.toJSON(),
                                success: function () {
                                    $("#editButton").html(TaxiHail.localize('Edit'));
                                    $("input").attr("disabled", true);
                                    settingschanged = false;
                                },
                            dataType: 'json'
                        });
                    } else {
                        $("#editButton").html(TaxiHail.localize('Edit'));
                        $("input").attr("disabled", true);
                    }
                
                }else {
                    var result = settings.validate(settings.attributes);
                    var $alert = $('<div class="alert alert-error" />');
                    _.each(result.errors, function (error) {
                        $alert.append($('<div />').text(this.localize(error.errorCode)));
                    }, this);
                    this.$('.errors').html($alert);
                }
                
            } else {
                $("#editButton").html(this.localize('Save'));
                $("input").attr("disabled", false);
            }
            
        },
        
        renderItem: function (model) {

            var settingsView = new TaxiHail.SettingsEditView({
                model: settings = new TaxiHail.Settings(model.get('settings')) 
            });

            this.$('div#settingsContent').prepend(settingsView.render().el);
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);
            var pickup = this.model.get('pickupAddress');
            
            pickup[$input.attr("name")] = $input.val();
            settingschanged = true;
        },
        
    });

}());


