(function () {

    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=edit]': 'edit'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        book: function (e) {
        e.preventDefault();

        this.model.save();
        },
        
        edit:function (e) {
            e.preventDefault();
            $("input").attr("disabled", !$("input").attr("disabled"));
            if ($("input").attr("disabled")) {
                $("#editButton").html(TaxiHail.localize('Edit'));
            } else {
                $("#editButton").html(TaxiHail.localize('Save'));
            }
            
        }
    });

}());


