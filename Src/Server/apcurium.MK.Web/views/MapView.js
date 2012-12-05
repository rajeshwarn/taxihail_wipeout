﻿(function () {
    TaxiHail.MapView = Backbone.View.extend({
        
        initialize : function () {
            _.bindAll(this, "geolocdone", "geoloc");
            this.streetZoomLevel = 17;
            this.cityZoomLevel = 12;
        },
        
        setModel: function(model, centerMapOnAddressChange) {
            if(this.model) {
                this.model.off(null, null, this);
            }
            this.model = model;

            this.model.on('change:pickupAddress', function (model, value) {
                this.updatePickup();
                if(model.isValidAddress('pickupAddress')) {
                    var location = new google.maps.LatLng(value.latitude, value.longitude);
                    if (centerMapOnAddressChange) {
                        this.zoomMap(this.streetZoomLevel);
                        this.centerMap(location);
                    }
                }
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                this.updateDropOff();
                if(model.isValidAddress('dropOffAddress')) {
                    var location = new google.maps.LatLng(value.latitude, value.longitude);
                    if(centerMapOnAddressChange) this.centerMap(location);
                }
            }, this);
            

            this.model.on('change:isPickupActive change:isDropOffActive', function (model, value) {
                this._target.set('visible',  this.model.get('isPickupActive') || this.model.get('isDropOffActive'));
            }, this);

            this.model.getStatus().on('change:vehicleLatitude change:vehicleLongitude', (function(self) {
                var isAssigned = false;
                return _.bind(function(model) {
                    this.updateVehiclePosition(model);
                    if(!isAssigned && model.get('vehicleLatitude') && model.get('vehicleLongitude') )
                    {
                        // Center the map on the taxi the first time we have coordinates
                        isAssigned = true;
                        this.centerMap(new google.maps.LatLng(model.get('vehicleLatitude'), model.get('vehicleLongitude')));
                    }
                }, self);
            }(this)));

            this.updatePickup();
            this.updateDropOff();
            this._target.set('visible',  this.model.get('isPickupActive') || this.model.get('isDropOffActive'));
            
        },
           
        render: function() {

            //default lat and long are defined in the default.aspx
            var mapOptions = {
                zoom: 12,
                center: new google.maps.LatLng(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude),
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                overviewMapControl: false,
                panControl: false,
                rotateControl: false,
                streetViewControl: false,
                scaleControl: false,
                zoomControlOptions: {
                    position: google.maps.ControlPosition.LEFT_CENTER
                }
            };

            this._map = new google.maps.Map(this.el, mapOptions);
            google.maps.event.addListener(this._map, 'dragend', this.geoloc);
            
            this._vehicleMarker = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: new google.maps.MarkerImage('assets/img/spacer.png', new google.maps.Size(1,1)),
                visible: false
            });
            var label = new TaxiLabel({
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
                visible: false
            });

            var onmapchanged = function() {
                var $container = $(target.getMap().getDiv()),
                    x = $container.width()/2 ,
                    y = $container.height() * 3 / 4,
                    projection = target.getProjection(),
                    position = projection.fromContainerPixelToLatLng(new google.maps.Point(x, y));

                target.set('position', position);
            };

            google.maps.event.addListener(this._map, 'bounds_changed', onmapchanged);
            google.maps.event.addListener(this._map, 'drag', onmapchanged);
            
            return this;
        },
        
        goToPickup: function () {
            if (this._pickupPin) this.centerMap(this._pickupPin.getPosition());
        },
        
        zoomMap: function (zoomLevel) {
            
            if (this._map.getZoom() < zoomLevel) {
                this._map.setZoom(zoomLevel);
            }
        },

        centerMap: function (location) {
            
            var projection = this._target.getProjection();
            if (projection) {
                //map ready center now
                this.centerFromProjection(projection, location);
                
            } else {
                //will center when the map is loaded
                var centerAfterLoad = _.bind(function () {
                    projection = this._target.getProjection();
                    this.centerFromProjection(projection, location);
                }, this);
                
                google.maps.event.addListenerOnce(this._map, 'tilesloaded', centerAfterLoad);
            }
        },
        
        centerFromProjection: function (projection, location) {
            var point = projection.fromLatLngToDivPixel(location);
            var center = new google.maps.Point(point.x, point.y - $(this._map.getDiv()).height() / 4);
            this._map.setCenter(projection.fromDivPixelToLatLng(center));
        },

        updateVehiclePosition: function(orderStatus){

            this._vehicleMarker.setPosition(new google.maps.LatLng(orderStatus.get('vehicleLatitude'), orderStatus.get('vehicleLongitude')));
            this._vehicleMarker.setVisible(true);
            this._vehicleMarker.set('text', orderStatus.get('vehicleNumber'));

        },

        updatePickup: function() {
            if(this._pickupPin) {
                this._pickupPin.setVisible(this.model.isValidAddress('pickupAddress'));
                if(this.model.isValidAddress('pickupAddress')) {
                    this._pickupPin.setPosition(new google.maps.LatLng(this.model.get('pickupAddress').latitude, this.model.get('pickupAddress').longitude));
                }
            }
        },

        updateDropOff: function() {
            if(this._dropOffPin) {
                this._dropOffPin.setVisible(this.model.isValidAddress('dropOffAddress'));
                if(this.model.isValidAddress('dropOffAddress')) {
                    this._dropOffPin.setPosition(new google.maps.LatLng(this.model.get('dropOffAddress').latitude, this.model.get('dropOffAddress').longitude));
                }
            }
        },
        
        geolocdone : function (result) {
            if (result && result.length) {
                if (this.model.get('isPickupActive')) {
                    this.model.set('pickupAddress', result[0]);
                } else {
                    this.model.set('dropOffAddress', result[0]);
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

    var TaxiLabel = function(opt_options) {
        // Initialization
        this.setValues(opt_options);

        // TaxiLabel specific
        var span = this.span_ = document.createElement('span');
        span.style.cssText = 'position: relative; display:block; width:100%; top: 13px; white-space: nowrap; font-size: 24px; font-weight: bold; color: white; text-align: center;';
        var marker = this.marker_ = document.createElement('div');
        marker.style.cssText = 'position: relative; width: 106px; height:80px; left: -53px; top: -70px; background: url(assets/img/taxi_label.png);';
        marker.appendChild(span);
        var div = this.div_ = document.createElement('div');
        div.appendChild(marker);
        div.style.cssText = 'position: absolute; display: none;';
    };

    _.extend(TaxiLabel.prototype, new google.maps.OverlayView(), {
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

            // TaxiLabel is removed from the map, stop updating its position/text.
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
        div.style.cssText = 'width: 25px; height: 25px; background: url(assets/img/target.png) top left no-repeat;position: absolute; display: none';
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
            div.style.left = (position.x - 12) + 'px';
            div.style.top =  (position.y - 12) + 'px';
            div.style.display = 'block';
         }
    });

}());