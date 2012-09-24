// Authentication Service
// Methods: login, logout
// Events: loggedIn, loggedOut
(function () {

    var isLoggedIn = false;


    TaxiHail.auth = _.extend(Backbone.Events, {
        login: function (email, password) {
            return $.post('api/auth/credentials', {
                userName: email,
                password: password
            },_.bind(function () {
                isLoggedIn = true;
                this.trigger('change', isLoggedIn);
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
        account: null,
        initialize: function() {
            this.account = new TaxiHail.UserAccount();

            this.on('change', function(isLoggedIn){
                if(!isLoggedIn) this.account.clear();
            }, this);

            new TaxiHail.UserAccount().fetch({
                success: _.bind(function(model) {
                    isLoggedIn = true;
                    this.account.set(model.toJSON());
                    //this.account('change', isLoggedIn);
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