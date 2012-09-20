(function () {

    TaxiHail.HomeView = TaxiHail.TemplatedView.extend({
        events: {
        },

        render: function () {
            this.$el.html(this.renderTemplate());            
            return this;
        },

        showConfirmationMessage: function() {
        	this.$('#alert')
        		.addClass('alert')
        		.addClass('alert-success')
        		.html(this.localize('signup.confirmation'));
        }
    });

}());