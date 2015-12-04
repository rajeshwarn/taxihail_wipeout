(function(TaxiHail){

    var currentOrderKey = "TaxiHail.currentOrder",
        OrderService = function () { };


    _.extend(OrderService.prototype, {
        setCurrentOrder: function (orderModel) {
            TaxiHail.localStorage.setItem(currentOrderKey, JSON.stringify(orderModel));
        },

        getCurrentOrder: function () {
            var item = TaxiHail.localStorage.getItem(currentOrderKey);
            if(item) {
                return new TaxiHail.Order(JSON.parse(item));
            }
            return null;
        },

        clearCurrentOrder: function () {
            TaxiHail.localStorage.removeItem(currentOrderKey);
        },

        validate: function (order, forError) {
            order.settings = order.settings || {};
            order.settings.serviceType = TaxiHail.auth.account.serviceType;
            order.settings.vehicleTypeId = TaxiHail.auth.account.vehicleTypeId;

            return $.ajax({
                type: 'POST',
                url: TaxiHail.parameters.apiRoot + "/account/orders/validate/" + forError,
                data: JSON.stringify(order) ,
                contentType: 'application/json'
            });
        }
    });

    TaxiHail.orderService = new OrderService();

}(TaxiHail));