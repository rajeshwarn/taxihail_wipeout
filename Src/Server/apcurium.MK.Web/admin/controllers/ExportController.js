(function()
{
    var Controller = TaxiHail.ExportController = TaxiHail.Controller.extend({

        initialize: function () {

            this.ready();
        },

        exportAccounts: function () {

            return new TaxiHail.ExportOrdersOrAccountsView();
        },

        exportOrders: function () {

            return new TaxiHail.ExportOrdersOrAccountsView();
        }
    });
}());