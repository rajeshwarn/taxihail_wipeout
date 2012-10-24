// Geolocation service

(function(){

    var isSupported = navigator.geolocation;

    TaxiHail.geolocation = {

        getCurrentPosition: function (searchPopular) {
            
            // Try W3C Geolocation (Preferred)
            var defer = $.Deferred();
            
            if (this.currentAddress) {
                defer.resolve(this.currentAddress);
            }else {
                if (isSupported) {

                    navigator.geolocation.getCurrentPosition(_.bind(function (position) {
                        
                        TaxiHail.geocoder.geocode(position.coords.latitude, position.coords.longitude)
                            .done(_.bind(function (result) {

                                if (result && result.length) {
                                    this.currentAddress = result[0];
                                    defer.resolve(result[0]);
                                }
                                else defer.reject();

                            }, this))
                            .fail(defer.reject);
                        
                        if(searchPopular == true) {
                            $.ajax({
                                url: 'api/popularaddresses/' + position.coords.longitude + '/' + position.coords.latitude,
                                dataType: 'json',
                                success: _.bind(function (e) {
                                    if (e) {
                                        this.currentAddress = e;
                                        defer.resolve(e);
                                    }
                                }, this)
                            });
                        }
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
