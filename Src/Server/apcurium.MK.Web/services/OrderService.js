(function(){

    var currentOrderKey = "TaxiHail.currentOrder",
        OrderService = function () {
            if (Modernizr.localstorage) {
                this.store = window.localStorage;
            } else {
                this.store = new Persist.Store('Order store');
            }
        };


    _.extend(OrderService.prototype, {
        setCurrentOrder: function (orderModel) {
            if (Modernizr.localstorage) {
                this.store.setItem(currentOrderKey, JSON.stringify(orderModel));
            } else {
                this.store.set(currentOrderKey, JSON.stringify(orderModel));
            }
        },

        getCurrentOrder: function () {
            var item = null;
            if (Modernizr.localstorage) {
                 item = this.store.getItem(currentOrderKey);
            } else {
                item = this.store.get(currentOrderKey);
            }
            if(item) {
                var attributes = JSON.parse(item);
                return new TaxiHail.Order(JSON.parse(item));
            }
            return null;
        },

        clearCurrentOrder: function () {
            if (Modernizr.localstorage) {
                this.store.removeItem(currentOrderKey);
            } else {
                this.store.remove(currentOrderKey);
            }
        }
    });

    TaxiHail.orderService = new OrderService();

}());