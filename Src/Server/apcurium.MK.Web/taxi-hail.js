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
        /* Localizations */["Home", "Book", "Login", "AddressSelection", "BookingConfirmation", "SettingsEdit", "Signup", "LoginStatus", "Map", "BookingStatus", "Profile", "UpdatePassword", "ResetPassword", "OrderHistory", "Global"],
        /* Templates*/["Home", "Book", "Login", "AddressSelection", "AddressItem", "AddressControl", "AddressSearch", "BookingConfirmation", "SettingsEdit", "Signup", "LoginStatus", "BookingStatus", "Profile", "UserAccount", "UpdatePassword", "ResetPassword", "OrderHistory"],
        function () {

            // Application starts here
            TaxiHail.app = new TaxiHail.App();
            Backbone.history.start();
        });
});