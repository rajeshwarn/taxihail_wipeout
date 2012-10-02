(function () {

    TaxiHail.LoginStatusView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=logout]': 'logout',
            'click [data-action=goToUserAccount] ': 'goToUserAccount',
        },

        initialize: function () {
            this.model.on('change', this.render, this);
        },

        render: function () {

            var data = _.extend(this.model.toJSON(), {
                isLoggedIn: TaxiHail.auth.isLoggedIn()
            });

            this.$el.html(this.renderTemplate(data));

            return this;
        },

        logout: function (e) {
            e.preventDefault();
            TaxiHail.auth.logout();
        },
        
        goToUserAccount : function (e) {
            e.preventDefault();
            TaxiHail.app.navigate('useraccount', { trigger: true });
        }

    });

}());