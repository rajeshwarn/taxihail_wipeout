(function() {
    TaxiHail.GetTheAppView = TaxiHail.TemplatedView.extend({
    
        

        initialize:function () {
        },

        render: function() {
            var thisthis = this;
            var x = new TaxiHail.CompanySettings();
            x.fetch({ data: { appSettingsType: 1 } }).success(function() {

                var settings = x.toJSON();
                thisthis.$el.html(thisthis.renderTemplate(
                    {
                        PlayLink: settings["Store.PlayLink"],
                        AppleLink: settings["Store.AppleLink"]
                    }));
            });
            

            return this;
        },
       
    });


})();