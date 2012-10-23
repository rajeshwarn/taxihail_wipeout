// Geolocation service

(function(){

    var isSupported = navigator.geolocation;

    TaxiHail.geolocation = {

        getCurrentPosition: function () {
            
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();
            
            if (this.currentAddress) {
                defer.resolve(this.currentAddress);
            }else {
                if (isSupported) {

                    navigator.geolocation.getCurrentPosition(_.bind(function (position) {
                        
                        // search for popular address in range
                        
                        var popular = new TaxiHail.CompanyPopularAddressCollection();
                        var addressInRange = new Backbone.Collection();
                       /* popular.fetch({
                            url: 'api/admin/popularaddresses',
                            success: _.bind(function(collection, resp) {
                                _.each(popular.models, _.bind(function(address) {
                                    if (TaxiHail.math.distanceBeetweenTwoLatLgt(address.attributes.latitude, address.attributes.longitude, position.coords.latitude, position.coords.longitude) < TaxiHail.parameters.GeolocPopularRange) {
                                        addressInRange.add(address);
                                        alert(TaxiHail.math.distanceBeetweenTwoLatLgt(address.attributes.latitude, address.attributes.longitude, position.coords.latitude, position.coords.longitude).toString());
                                    }
                                }, this));
                            }, this)
                        });*/

                        TaxiHail.geocoder.geocode(position.coords.latitude, position.coords.longitude)
                            .done(_.bind(function (result) {

                                if (result && result.length) {
                                    this.currentAddress = result[0];
                                    defer.resolve(result[0]);
                                }
                                else defer.reject();

                            }, this))
                            .fail(defer.reject);

                    }, this), defer.reject);
                }
                else {
                    window.setTimeout(defer.reject, 1);
                }
            }
            
            return defer.promise();
        }
    };
}());
