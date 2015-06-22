(function () {

    TaxiHail.Order = Backbone.Model.extend({

        idAttribute: 'orderId',
        urlRoot: 'api/account/orders',

        validateOrder: function (forError) {
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

        cancel: function () {
            return $.post(this.url() + '/cancel', {
                orderId: this.id
            }, function(){}, 'json');
        },

        sendReceipt: function() {
            return $.post(this.url() + '/sendreceipt', {}, function(){}, 'json');
        },

        initiateCallToDriver: function () {
            return $.get(this.url() + '/calldriver', {}, function () { }, 'json');
        },

        sendMessageToDriver: function(vehicleNumber, message) {
            return $.ajax({
                type: 'POST',
                url: 'api/vehicle/' + vehicleNumber + '/message',
                data: JSON.stringify({
                    message: message
                }),
                contentType: 'application/json'
            }, this);
        },

        getStatus: function() {
            return this._status || (this._status = new TaxiHail.OrderStatus({
                id: this.id
            }));
        },

        isValidAddress: function(attr) {
            var value = this.get(attr);
            return !!(value && value.fullAddress && value.latitude && value.longitude);
        },

        isPayingWithAccountCharge: function () {
            var settings = this.get('settings');

            return this.isChargeAccount(settings.chargeTypeId);
        },

        isChargeAccount: function(chargeType) {
            return chargeType != null
                && chargeType != ''
                && chargeType == 2;
        },

        isPayingWithPayPal: function () {
            var settings = this.get('settings');
            return settings.chargeTypeId != null
                && settings.chargeTypeId != ''
                && settings.chargeTypeId == 4;
        },

        isPayingWithCoF: function () {
            var settings = this.get('settings');
            return settings.chargeTypeId != null
                && settings.chargeTypeId != ''
                && settings.chargeTypeId == 3;
        },

        switchOrderToNextDispatchCompany: function () {
            return $.ajax({
                type: 'POST',
                url: this.url() + "/switchDispatchCompany",
                data: JSON.stringify({
                    nextDispatchCompanyKey: this._status.get('nextDispatchCompanyKey'),
                    nextDispatchCompanyName: this._status.get('nextDispatchCompanyName')
                }),
                contentType: 'application/json'
            },this);
        },

        ignoreDispatchCompanySwitch: function () {
            return $.ajax({
                type: 'POST',
                url: this.url() + "/ignoreDispatchCompanySwitch",
                contentType: 'application/json'
            });
        },

        fetchQuestions: function (accountChargeNumber, customerNumber) {
            return $.get('api/admin/accountscharge/' + accountChargeNumber + '/' + customerNumber + '/true', function () { }, 'json');
        }
    });
}());