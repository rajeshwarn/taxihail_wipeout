(function () {

    TaxiHail.Order = Backbone.Model.extend({

        idAttribute: 'orderId',
        urlRoot: 'api/account/orders',

        validateOrder: function (forError)
        {
            return TaxiHail.orderService.validate(this, forError);
        },

        save: function(key, value, options) {

            if (_.isObject(key) || key == null) {
                attrs = key;
                options = value;
            } else {
                attrs = {};
                attrs[key] = value;
            }
            options = options ? _.clone(options) : {};

            var success = options.success;
            options.success = function(model, resp) {
                TaxiHail.orderService.setCurrentOrder(model);
                if(success) {
                    success(model, resp);
                }
            };

            if (_.isObject(key) || key == null) {
                Backbone.Model.prototype.save.call(this, key, options);
            } else {
                Backbone.Model.prototype.save.call(this, key, value, options);
            }
        },

        saveLocal: function() {

            TaxiHail.orderService.setCurrentOrder(this);
        },

        destroyLocal: function() {
            TaxiHail.orderService.clearCurrentOrder();
        },

        cancel: function() {

            return $.post(this.url() + '/cancel', {
                orderId: this.id
            }, function(){}, 'json');

        },

        sendReceipt: function() {
            return $.post(this.url() + '/sendreceipt', {}, function(){}, 'json');
        },

        getStatus: function() {
            return this._status || (this._status = new TaxiHail.OrderStatus({
                id: this.id
            }));
        },

        getAvailableVehicles: function () {
            return new TaxiHail.AvailableVehicleCollection([], { latitude: 0, longitude: 0 });
        },

        isValidAddress: function(attr) {
            var value = this.get(attr);
            return !!(value && value.fullAddress && value.latitude && value.longitude);
        },

        isPayingWithAccountCharge: function () {
            var settings = this.get('settings');
            return settings.chargeTypeId != null
                && settings.chargeTypeId != ''
                && settings.chargeTypeId == 2
        },
        fetchQuestions: function (accountChargeNumber) {
            return $.get('api/admin/accountscharge/' + accountChargeNumber + '/true', function () { }, 'json');
        }
    });

}());