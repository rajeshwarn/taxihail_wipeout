var TaxiHail = {
    resources: {}
};

TaxiHail.loader = {

    load: function (resources, views, callback) {

        var deferreds = [];

        // Load resource sets
        $.each(resources, function (index, resourceSet) {
            deferreds.push($.get('localization/' + resourceSet + '.json', function (data) {
                TaxiHail.addResourceSet(resourceSet, data);
                if (TaxiHail[resourceSet + 'View']) {
                    TaxiHail[resourceSet + 'View'].prototype.resourceSet = resourceSet;
                }
            }));
        });

        // Load templates
        $.each(views, function (index, view) {
            if (TaxiHail[view + 'View']) {
                deferreds.push($.get('templates/' + view + '.html', function (data) {
                    TaxiHail[view + 'View'].prototype.template = Handlebars.compile(data);
                }, 'html'));
            } else {

            }
        });

        $.when.apply(null, deferreds).done(callback);
    }

};
$(function () {
    TaxiHail.loader.load(
        /* Localizations */ ["Home", "Book", "Login", "AddressSelection", "BookingConfirmation", "SettingsEdit","Signup", "Global"],
        /* Templates*/["Home", "Book", "Login", "AddressSelection", "AddressItem", "AddressControl", "AddressSearch", "BookingConfirmation", "SettingsEdit", "Signup"],
        function () {

            // Application starts here
            TaxiHail.app = new TaxiHail.App();

            // Redirect to login in case of HTTP 401
            $(document).ajaxError(function (e, jqxhr, settings, exception) {
                if (jqxhr.status === 401 /* Unauthorized */) {
                    TaxiHail.app.navigate('login', { trigger: true });
                }
            });

            Backbone.history.start();
        });
});