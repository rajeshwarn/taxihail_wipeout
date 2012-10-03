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

        initialize: function(oninitialized, context) {
            this.account = new TaxiHail.UserAccount();

            this.on('change', function(isLoggedIn){
                if(!isLoggedIn) this.account.clear();
            }, this);

            // Fetch user account
            // We use a different instance of UserAccount
            // In order to be able to set the isLoggedIn flag
            // Before setting the auth.account attributes
            new TaxiHail.UserAccount().fetch({
                success: _.bind(function(model) {
                    isLoggedIn = true;
                    this.account.set(model.toJSON());
                    if(oninitialized) {
                        oninitialized.call(context, isLoggedIn);
                    }
                    this.trigger('init', isLoggedIn);
                }, this)
            });



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