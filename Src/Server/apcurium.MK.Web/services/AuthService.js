// Authentication Service
// Methods: login, logout
// Events: loggedIn, loggedOut
var isLogged = false;
(function () {

    TaxiHail.auth = _.extend(Backbone.Events, {
        login: function (email, password) {
            isLogged = true;
            return $.post('api/auth/credentials', {
                userName: email,
                password: password
            },_.bind(function () {
                this.trigger('loggedIn');
            }, this), 'json');
        },

        logout: function () {
            isLogged = false;
            return $.post('api/auth/logout', _.bind(function () {
                this.trigger('loggedOut');                
            }, this), 'json');
        },
        
        isLogged : function() {
            return isLogged;
        }
    
    });

} ());