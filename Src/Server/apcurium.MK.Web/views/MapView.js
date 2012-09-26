(function () {
    
    TaxiHail.MapView = Backbone.View.extend({
        
        events: {
            'mouseup': 'mouseup'
        },
        
        initialize : function () {
            _.bindAll(this, "geolocdone");
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
                zoomControlOptions: {
                    position: google.maps.ControlPosition.LEFT_CENTER
                }

            };
            this._map = new google.maps.Map(this.el, mapOptions);
            this._vehicleMarker = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: new google.maps.MarkerImage('assets/img/taxi_label.png', new google.maps.Size(106,80)),
                visible: false
            });
            var label = new Label({
               map: this._map
            });
            label.bindTo('position', this._vehicleMarker, 'position');
            label.bindTo('text', this._vehicleMarker, 'text');
            label.bindTo('visible', this._vehicleMarker, 'visible');

            return this;

        },

        updateVehiclePosition: function(orderStatus){

            this._vehicleMarker.setPosition(new google.maps.LatLng(orderStatus.get('vehicleLatitude'), orderStatus.get('vehicleLongitude')));
            this._vehicleMarker.setVisible(true);
            this._vehicleMarker.set('text', orderStatus.get('vehicleNumber'));

        },
        
        geolocdone : function (result) {
            if (result.addresses && result.addresses.length) {
                if (this.model.get('isPickupBtnSelected')) {
                    this.model.set('pickupAddress', result.addresses[0]);
                } else {
                    this.model.set('dropOffAddress', result.addresses[0]);
                }
            }
        },
        
        mouseup: function () {
            if (this.model.get('isPickupBtnSelected')) {
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
            
        },

        addMarker : function(location, iconImage) {
            return new google.maps.Marker({
                position: location,
                map: this._map,
                icon: iconImage
            } );
        }

        
    });

    var Label = function(opt_options) {
        // Initialization
        this.setValues(opt_options);

        // Label specific
        var span = this.span_ = document.createElement('span');
        span.style.cssText = 'position: relative; left: -50%; top: -66px; white-space: nowrap; font-size: 24px; font-weight: bold; color: white';

        var div = this.div_ = document.createElement('div');
        div.appendChild(span);
        div.style.cssText = 'position: absolute; display: none';
    };

    _.extend(Label.prototype, new google.maps.OverlayView(), {
        onAdd: function() {
            var pane = this.getPanes().overlayLayer;
            pane.style.zIndex = 121;
             pane.appendChild(this.div_);

             // Ensures the label is redrawn if the text or position is changed.
             var me = this;
             this.listeners_ = [
               google.maps.event.addListener(this, 'position_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'text_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'visible_changed',
                   function() { me.draw(); })
             ];
         },
         onRemove: function() {
            this.div_.parentNode.removeChild(this.div_);

            // Label is removed from the map, stop updating its position/text.
            for (var i = 0, I = this.listeners_.length; i < I; ++i) {
               google.maps.event.removeListener(this.listeners_[i]);
            }
        },
        draw: function() {
            if(!this.get('visible')) {
                this.div_.style.display = 'none';
                return;
            }

            var projection = this.getProjection();
            var position = projection.fromLatLngToDivPixel(this.get('position'));

            var div = this.div_;
            div.style.left = position.x + 'px';
            div.style.top = position.y + 'px';
            div.style.display = 'block';

            this.span_.innerHTML = this.get('text') || '';
         }
    });



}());