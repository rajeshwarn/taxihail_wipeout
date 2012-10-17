var TaxiHail = {
    resources: {},
    parameters: {}
};

TaxiHail.loader = {

    load: function (callback) {

        var deferreds = [];

        $.when.apply(null, deferreds).done(callback);
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