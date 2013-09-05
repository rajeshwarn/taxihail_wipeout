var TaxiHail = {
    resources: {},
    parameters: {}
};

TaxiHail.localStorage = window.localStorage;
TaxiHail.sessionStorage = window.sessionStorage;

TaxiHail.loader = {

    load: function (callback) {

        var deferreds = [];

        $.when.apply(null, deferreds).done(callback);
    }

};
$(function () {
    TaxiHail.loader.load(function () {

            _.each(Handlebars.templates, function(value, key, list) {
                if(TaxiHail[key + 'View']) {
                    TaxiHail[key + 'View'].prototype.template = Handlebars.compile(value);
                }
            });
            
            _.each(TaxiHail.resources, function (data, resourceSet, list) {
                if (TaxiHail[resourceSet + 'View']) {
                    TaxiHail[resourceSet + 'View'].prototype.resourceSet = resourceSet;
                }
            });

            // Application starts here
            // If user is logged in, we need to load its Account before we continue

            if(TaxiHail.parameters.isLoggedIn) {

                new TaxiHail.UserAccount().fetch({
                    success: function(model) {

                        TaxiHail.app = new TaxiHail.App({
                            account: model
                        });
                        Backbone.history.start();

                    }
                });
                
            } else {
                TaxiHail.app = new TaxiHail.App();
                Backbone.history.start();
            }
        });
});