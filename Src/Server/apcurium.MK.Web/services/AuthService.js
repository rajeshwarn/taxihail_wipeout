// Authentication Service
// Methods: login, logout
// Events: loggedIn, loggedOut

(function () {

    TaxiHail.auth = _.extend(Backbone.Events, {
        login: function (email, password) {
            return $.post('api/auth/credentials', {
                userName: email,
                password: password
            },_.bind(function () {
                this.trigger('loggedIn');
            }, this), 'json');
        },

        logout: function () {
            return $.post('api/auth/logout', _.bind(function () {
                this.trigger('loggedOut');                
            }, this), 'json');
        }
    });

} ());