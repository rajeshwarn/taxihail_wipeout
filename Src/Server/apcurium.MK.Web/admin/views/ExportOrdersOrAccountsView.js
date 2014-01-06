(function ()
{

    var View = TaxiHail.ExportOrdersOrAccountsView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=confirmExport]': 'exportData',
            'click [data-action=eraseStartTime]': 'onEraseStartTimeClick',
            'click [data-action=eraseEndTime]': 'onEraseEndTimeClick'
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            return this;
        },

        exportData: function (form) {

            var startDate = this.$('#book-later-date-start').val();
            var endDate = this.$('#book-later-date-end').val();

            console.log("startDate = " + startDate);
            console.log("endDate = " + endDate);
        },

        onEraseStartTimeClick: function (e) {
            console.log("ExportOrdersOrAccountsView.onEraseStartTimeClick");
            e.preventDefault();
            this.$('#book-later-date-start').val('');
        },

        onEraseEndTimeClick: function (e) {
            console.log("ExportOrdersOrAccountsView.onEraseEndTimeClick");
            e.preventDefault();
            this.$('#book-later-date-end').val('');
        }

    });
}());