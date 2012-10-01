(function(){

    TaxiHail.BookLaterView = TaxiHail.TemplatedView.extend({

        render: function() {

            var now = new Date();
            var data = _.extend(this.model.toJSON(), {
                today: now.getFullYear() + '-' + (now.getMonth() + 1) + '-' + now.getDate()
            });

            this.$el.html(this.renderTemplate(data));
            this.$('[data-role=datepicker]').datepicker();
            this.$('[data-role=timepicker]').timepicker();

            return this;

        }

    });

}());