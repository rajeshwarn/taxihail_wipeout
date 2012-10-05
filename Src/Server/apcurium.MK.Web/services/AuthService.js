// Authentication Service
// Methods: login, logout
// Events: loggedIn, loggedOut
(function () {

    var isLoggedIn = false;


    TaxiHail.auth = _.extend(Backbone.Events, {
        account: null,
        login: function (email, password, url) {
            return $.post('api/auth/credentials', {
                userName: email,
                password: password
            },_.bind(function () {
                isLoggedIn = true;
                
                this.trigger('change', isLoggedIn, url);
            }, this), 'json');
        },

        logout: function () {
            isLogged = false;
            return $.post('api/auth/logout', _.bind(function () {
                isLoggedIn = false;
                this.trigger('change', isLoggedIn);
            }, this), 'json');
        },
        
        isLoggedIn : function() {
            return isLoggedIn;
        },

        initialize: function(account) {
            if(account == null) {
                isLoggedIn = false;
                this.account = new TaxiHail.UserAccount();
            }
            else {
                isLoggedIn = true;
                this.account = new TaxiHail.UserAccount(account.toJSON());
            }

            this.on('change', function(isLoggedIn){
                if(!isLoggedIn) this.account.clear();
            }, this);
        }
    
    });

    $(document).ajaxError(function (e, jqxhr, settings, exception) {
        if (jqxhr.status === 401  /*Unauthorized*/ ) {
             if(isLoggedIn) {
                isLoggedIn = false;
                TaxiHail.auth.trigger('change', isLoggedIn);
             }
         }
    });

} ());