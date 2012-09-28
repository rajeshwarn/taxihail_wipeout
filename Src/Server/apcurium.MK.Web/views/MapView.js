(function () {
    TaxiHail.MapView = Backbone.View.extend({
        
        events: {
        },
        
        initialize : function () {
            _.bindAll(this, "geolocdone", "geoloc");
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
                    this._pickupPin.setVisible(true);
                }
                this.centerMap(location);
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                if (this._dropOffPin) {
                    this._dropOffPin.setPosition(location);
                    this._dropOffPin.setVisible(true);
                }
                this.centerMap(location);
            }, this);
            

            this.model.on('change:isPickupActive change:isDropOffActive', function (model, value) {
                if (model.get('isPickupActive') || model.get('isDropOffActive')) {
                    this._target.set('visible', true);
                } else {
                    this._target.set('visible', false);
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
            google.maps.event.addListener(this._map, 'dragend', this.geoloc);
            
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

            this._pickupPin = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: 'assets/img/pin_green.png',
                visible: false
            });

            this._dropOffPin = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: 'assets/img/pin_red.png',
                visible: false
            });

            var target = this._target = new Target({
                position: this._map.getCenter(),
                map: this._map,
                visible: true
            });

            var onmapchanged = function() {
                var $container = $(target.getMap().getDiv()),
                    x = $container.width()/2 ,
                    y = $container.height() * 3 / 4,
                    projection = target.getProjection(),
                    position = projection.fromContainerPixelToLatLng(new google.maps.Point(x, y));

                target.set('position', position);
            };
            
            var getdefaultlocation = _.bind(function () {
                $.get('api/settings/defaultlocation',
                    _.bind(function (address) {
                        this.centerMap(new google.maps.LatLng(address.latitude, address.longitude));
                        google.maps.event.clearListeners(this._map, 'tilesloaded'); //to refine if others listener subscribe
                    }, this),
                    "json");
            }, this);

            google.maps.event.addListener(this._map, 'bounds_changed', onmapchanged);
            google.maps.event.addListener(this._map, 'drag', onmapchanged);
            google.maps.event.addListener(this._map, 'tilesloaded', getdefaultlocation);
            
            return this;
        },
        

        centerMap: function(location) {
            var projection = this._target.getProjection(),
                    point = projection.fromLatLngToDivPixel(location),
                    center = new google.maps.Point(point.x, point.y - $(this._map.getDiv()).height() / 4 );

            this._map.setCenter(projection.fromDivPixelToLatLng(center));
        },

        updateVehiclePosition: function(orderStatus){

            this._vehicleMarker.setPosition(new google.maps.LatLng(orderStatus.get('vehicleLatitude'), orderStatus.get('vehicleLongitude')));
            this._vehicleMarker.setVisible(true);
            this._vehicleMarker.set('text', orderStatus.get('vehicleNumber'));

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
            var position = this._target.get('position');
            if (this.model.get('isPickupActive') || this.model.get('isDropOffActive')) {
                if (this.model.get('isPickupActive') && this._pickupPin) {
                    this._pickupPin.setPosition(position);
                    this._pickupPin.setVisible(true);
                } else if (this.model.get('isDropOffActive') && this._dropOffPin) {
                    this._dropOffPin.setPosition(position);
                    this._dropOffPin.setVisible(true);
                }

                TaxiHail.geocoder.geocode(position.lat(), position.lng())
                    .done(this.geolocdone);
            }
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

    var Target = function(options) {
        // Initialization
        this.setValues(options);

        var div = this.div_ = document.createElement('div');
        div.style.cssText = 'width: 50px; height: 50px; background: url(themes/generic/img/target.png) top left no-repeat;position: absolute; display: none';
    };

    _.extend(Target.prototype, new google.maps.OverlayView(), {
        onAdd: function() {
            var pane = this.getPanes().overlayLayer;
            pane.style.zIndex = 121;
            pane.appendChild(this.div_);

             var me = this;
             this.listeners_ = [
             google.maps.event.addListener(this, 'position_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'visible_changed',
                   function() { me.draw(); })
             ];
         },
         onRemove: function() {
            this.div_.parentNode.removeChild(this.div_);

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
            div.style.left = (position.x -25) + 'px';
            div.style.top = (position.y  -25) + 'px';
            div.style.display = 'block';
         }
    });

}());