var TaxiHail = {
    resources: {},
    parameters: {}
};

// Assign window.locaStorage to TaxiHail.localStorage
// In IE7 Browser Mode (IE Developer Tools) window.localStorage is available but throws errors
// It can be disabled in Internet Options by disabling DOM Storage.
// However, when DOM Storage is disabled, window.locaStorage is null and cannot be assigned
// That's why we use TaxiHail.localStorage instead.

TaxiHail.localStorage = window.localStorage;
TaxiHail.sessionStorage = window.sessionStorage;

TaxiHail.loader = {

    load: function (callback) {
        // Disable caching to fix an issue on IE where the pages were not always up to date
        $.ajaxSetup({ cache: false });

        Modernizr.load([{
            test: window.JSON && Modernizr.localstorage,
            nope: ['assets/js/json2.js', 'assets/js/storage-polyfill.js'],
            complete: callback
        },
        {
            test : $('html').is('.lt-ie8'),
            yep : ['assets/js/jquery.jreject.min.js', 'assets/css/jquery.jreject.css'],
            complete: function() {
                $('html').is('.lt-ie8') && $.reject({
                    reject: { all: true },
                    imagePath: './assets/img/jreject/'
                });
                return false;
            }
          }]);
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
            
            // initialize fb
            window.fbAsyncInit = function () {
                FB.init({
                    appId: TaxiHail.parameters.facebookAppId, // App ID
                    channelUrl: 'http://localhost/apcurium.MK.Web/channel.html', // Path to your Channel File
                    status: true, // check login status
                    cookie: true, // enable cookies to allow the server to access the session
                    xfbml: true  // parse XFBML
                });
            };
            // Load the SDK Asynchronously
            (function (d) {
                var js, id = 'facebook-jssdk', ref = d.getElementsByTagName('script')[0];
                if (d.getElementById(id)) { return; }
                js = d.createElement('script'); js.id = id; js.async = true;
                js.src = "//connect.facebook.net/en_US/all.js";
                ref.parentNode.insertBefore(js, ref);
            }(document));
            
            // initialize twitter asynchronously
            var twApiKey = "AaXTjhWMzbhEAzL1ac4N3g";
            (function (d) {
                var js, id = 'twitter-anywhere', ref = d.getElementsByTagName('script')[0];
                if (d.getElementById(id)) { return; }
                js = d.createElement('script'); js.id = id; js.async = true;
                
                js.src = "//platform.twitter.com/anywhere.js?id=" + twApiKey + "&v=1";
                ref.parentNode.insertBefore(js, ref);
            }(document));
        });
});
