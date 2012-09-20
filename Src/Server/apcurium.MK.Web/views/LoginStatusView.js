(function () {

    TaxiHail.LoginStatusView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=logout]': 'logout'
        },

        initialize: function () {
            this.model.on('change', this.render, this);
        },

        render: function () {

            if (this.model.isNew()) {
                // Account is not loaded
                // Hide the view
                this.$el.empty();
            } else {
                this.$el.html(this.renderTemplate(this.model.toJSON()));
            }

            return this;
        },

        logout: function (e) {
            e.preventDefault();
            TaxiHail.auth.logout();
        }

    });

}());