(function()
{
    var Controller = TaxiHail.ExportController = TaxiHail.Controller.extend({

        initialize: function () {

            this.ready();
        },

        exportAccounts: function () {

            return new TaxiHail.ExportAccountsView();
        },

        exportOrders: function () {

            return new TaxiHail.ExportOrdersView();
        },

        exportPromotions: function ()
        {
        	return new TaxiHail.ExportPromotionsView();
        }
    });
}());