(function(){

    var currentOrderKey = "TaxiHail.currentOrder",
        OrderService = function () {
            this.store = window.localStorage;
        };


    _.extend(OrderService.prototype, {
        setCurrentOrder: function (orderModel) {
            this.store.setItem(currentOrderKey, JSON.stringify(orderModel));
        },

        getCurrentOrder: function () {
            var item = this.store.getItem(currentOrderKey);
            if(item) {
                return new TaxiHail.Order(JSON.parse(item));
            }
            return null;
        },

        clearCurrentOrder: function () {
            this.store.removeItem(currentOrderKey);
        },

        validate: function (order) {
            return $.post('api/account/orders/validate', order.toJSON(), function () { }, 'json');
        }
    });

    TaxiHail.orderService = new OrderService();

}());