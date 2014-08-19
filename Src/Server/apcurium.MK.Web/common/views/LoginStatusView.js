(function () {

    TaxiHail.LoginStatusView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-action=logout]': 'logout'
        },

        initialize: function () {            
            this.model.on('change', this.render, this);
            this.model.on('checkloggedorsubscribe', this.checkloggedorsubscribe, this);
        },

        render: function () {

            var data = _.extend(this.model.toJSON(), {
                isSignupVisible: TaxiHail.parameters.isSignupVisible,
                isLoggedIn: TaxiHail.auth.isLoggedIn(),
                name: this.model.has('settings') ? this.model.get('settings').name : this.model.get('name')
            });

            this.$el.html(this.renderTemplate(data));

            return this;
        },
        
       

        logout: function (e) {
            e.preventDefault();
            TaxiHail.auth.logout();
        }

    });

}());