(function () {
    var settings ;
    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=edit]': 'edit',
            
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.$("input").attr("disabled", true);
            this.renderItem(this.model);
            
            return this;
        },
        
        book: function (e) {
        e.preventDefault();

        this.model.save();
        },
        
        edit:function (e) {
            e.preventDefault();
            //$("input").attr("disabled", !$("input").attr("disabled"));
            if (!$("input").attr("disabled")) {
                if (settings.isValid()) {
                    
                
                jQuery.ajax({
                    type: 'PUT',
                    url: 'api/account/bookingsettings',
                    data: settings.toJSON(),
                    success: function () {
                        $("#editButton").html(TaxiHail.localize('Edit'));
                        $("input").attr("disabled", true);
                    },
                    dataType: 'json'
                });
                }
                
            } else {
                $("#editButton").html(this.localize('Save'));
                $("input").attr("disabled", false);
                
               
            }
            
        },
        
        callback : function () {
            
        },
        
        
        
        renderItem: function (model) {

            var settingsView = new TaxiHail.SettingsEditView({
                model: settings = new TaxiHail.Settings(model.get('settings')) 
            });

            this.$('div#settingsContent').prepend(settingsView.render().el);
        }
        
    });

}());


