(function ()
{

    var View = TaxiHail.ExportAccountsView = TaxiHail.TemplatedView.extend({

        events: {            
            'click [data-action=eraseStartTime]': 'onEraseStartTimeClick',
            'click [data-action=eraseEndTime]': 'onEraseEndTimeClick'
        },

        render: function () {

            now = new Date();
            today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
            this.$el.html(this.renderTemplate());
            this.$('[data-role=datepicker]').datepicker({});

            return this;
        },       

        onEraseStartTimeClick: function (e) {
            e.preventDefault();
            this.$('#book-later-date-start').val('');
        },

        onEraseEndTimeClick: function (e) {
            e.preventDefault();
            this.$('#book-later-date-end').val('');
        },

 
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);
}());