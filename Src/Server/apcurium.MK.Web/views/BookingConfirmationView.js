(function () {

    TaxiHail.BookingConfirmationView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=book]': 'book',
            'click [data-action=edit]': 'edit',
            'change :text': 'onPropertyChanged',
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
                jQuery.ajax({
                    type: 'PUT',
                    url: 'api/account/bookingsettings',
                    data: this.model.get('settings'),
                    success: function () {
                        
                    },
                    dataType: 'json'
                });
            } else {
                $("#editButton").html(TaxiHail.localize('Save'));
                
               
            }
            
        },
        
        callback : function () {
            
        },
        
        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget);

            var settings = this.model.get('settings');
            settings[$input.attr('name')] = $input.val();
        }
        
    });

}());


