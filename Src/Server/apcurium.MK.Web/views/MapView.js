(function () {
    
    TaxiHail.MapView = Backbone.View.extend({
        
        events: {
        },
        
        initialize : function () {
            _.bindAll(this, "geolocdone");
            _.bindAll(this, "geoloc");
        },
        
        
        
        setModel: function(model) {
            if(this.model) {
                this.model.off(null, null, this);
            }
            this.model = model;

            this.model.on('change:pickupAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                if (this._pickupPin) {
                    this._pickupPin.setPosition(location);
                } else {
                    this._pickupPin = this.addMarker(location, 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');
                }
                this._map.setCenter(location);
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                if (this._dropOffPin) {
                    this._dropOffPin.setPosition(location);
                } else {
                    this._dropOffPin = this.addMarker(location, 'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
                }
            }, this);
            
        },
           
            
        render: function() {

            var mapOptions = {
                zoom: 12,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP,

            };
            this._map = new google.maps.Map(this.el, mapOptions);
            google.maps.event.addListener(this._map, 'dragend', this.geoloc);


            return this;

        },
        
        geolocdone : function (result) {
            if (result.addresses && result.addresses.length) {
                if (this.model.get('isPickupActive')) {
                    this.model.set('pickupAddress', result.addresses[0]);
                } else {
                    this.model.set('dropOffAddress', result.addresses[0]);
                }
            }
        },
        
        geoloc: function () {
            if (this.model.get('isPickupActive') == true || this.model.get('isDropOffActive') == true) {
                if (this.model.get('isPickupActive')) {
               if (this._pickupPin) {
                this._pickupPin.setPosition(this._map.getCenter());
            } else {
                this._pickupPin = this.addMarker(this._map.getCenter(), 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');
            }
            
            TaxiHail.geocoder.geocode(this._pickupPin.getPosition().Xa, this._map.getCenter().Ya)
                        .done(this.geolocdone);
            } else {
                if (this._dropOffPin) {
                    this._dropOffPin.setPosition(this._map.getCenter());
                } else {
                    this._dropOffPin = this.addMarker(this._map.getCenter(), 'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
                }

                TaxiHail.geocoder.geocode(this._dropOffPin.getPosition().Xa, this._map.getCenter().Ya)
                            .done(this.geolocdone);
            }
            }
            
            
        },

        addMarker : function(location, iconImage) {
            return new google.maps.Marker({
                position: location,
                map: this._map,
                icon: iconImage
            } );
        }

        
    });

}());