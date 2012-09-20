(function() {

    TaxiHail.UserAccount = Backbone.Model.extend({
        events: {
            //'click [data-action=logout]': TaxiHail.auth.logout
        },
        
        url: 'api/account'
    });

}());