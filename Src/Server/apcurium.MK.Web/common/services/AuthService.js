﻿// Authentication Service
// Methods: login, logout
// Events: loggedIn, loggedOut
(function () {

    var isLoggedIn = false,
    currentOrderKey = "TaxiHail.currentOrder";

    TaxiHail.auth = _.extend({}, Backbone.Events, {
        account: null,
        login: function (email, password, url) {
            return $.post(TaxiHail.parameters.apiRoot + '/auth/login/password', {
                userName: email,
                password: password
            },_.bind(function (result) {
                isLoggedIn = true;

                var expire = new Date();
                expire.setTime(expire.getTime() + 3600000 * 24 * 365);
                // We set the local auth cookie.
                document.cookie = "ss-pid=" + result.sessionId + ";expires=" + expire.toGMTString();

                this.trigger('change', isLoggedIn, url);
            }, this), 'json');
        },

        logout: function () {
            isLogged = false;
            TaxiHail.localStorage.removeItem('fbId');
            TaxiHail.localStorage.removeItem(currentOrderKey);

            return $.post(TaxiHail.parameters.apiRoot + '/auth/logout', _.bind(function () {
                isLoggedIn = false;

                // We remove the local auth cookie
                document.cookie = "ss-pid=";

                this.trigger('change', isLoggedIn);
            }, this), 'json');
        },

        resetPassword: function(email) {
            return $.post(TaxiHail.parameters.apiRoot + '/accounts/resetpassword/' + email, {}, function () { }, 'json');
        },
        
        fblogin: function (url) {
          
                            FB.api('/me', _.bind(function (me) {
                                if (me.name) {

                                    $.ajax({
                                        headers: {          
                                            Accept : "application/json; charset=utf-8",         
                                            "Content-Type": "application/x-www-form-urlencoded; charset=utf-8"
                                        },
                                        url: TaxiHail.parameters.apiRoot + '/auth/login/facebook',
                                        type: 'POST',
                                        data: {
                                            userName: me.id,
                                            password: me.id
                                        }
                                    , dataType:'json'})
                                        
                                        .success(_.bind(function (d) {
                                            isLoggedIn = true;
                                            this.trigger('change', isLoggedIn,url);
                                        }, this))
                                        .error(function (e) {
                                            if (e.status == 401) {
                                                TaxiHail.localStorage.setItem('fbinfos', JSON.stringify(me));
                                                TaxiHail.localStorage.setItem('fbId', me.id);
                                                if (url) {
                                                    TaxiHail.app.navigate('signup/'+url, { trigger: true });
                                                } else {
                                                    TaxiHail.app.navigate('signup', { trigger: true });
                                                }
                                            }
                                        });
                                }
                            }, this));
        },
        
        twlogin: function (url) {

            twttr.anywhere(_.bind(function (T) {
                
                if (T.isConnected()) {
                    var me = T.currentUser;
                    $.post(TaxiHail.parameters.apiRoot + '/auth/login/twitter', {
                        userName: me.id,
                        password: me.id
                    }, 'json')
                        .success(_.bind(function () {
                            isLoggedIn = true;
                            this.trigger('change', isLoggedIn, url);
                        }, this))
                        .error(function (e) {
                            if (e.status == 401) {
                                TaxiHail.localStorage.setItem('twinfos', JSON.stringify(me));
                                TaxiHail.localStorage.setItem('twId', me.id);
                                if (url) {
                                    TaxiHail.app.navigate('signup/' + url, { trigger: true });
                                } else {
                                    TaxiHail.app.navigate('signup', { trigger: true });
                                }
                            }
                        });
                }
            }, this));
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