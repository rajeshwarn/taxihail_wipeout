(function () {

    TaxiHail.UserAccountView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=goToProfile]': 'goToProfile',
            'click [data-action=goToFavorites]': 'goToFavorites',
            'click [data-action=goToHistory]': 'goToHistory',
            'click [data-action=goToPassword]': 'goToPassword',
            'click .nav-tabs li>a': 'ontabclick'
        },

        initialize: function() {

           

        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.goToProfile();

            return this;
        },
        
        goToProfile: function (e) {
            if (e) {
                e.preventDefault();
            }
            this._profile = new TaxiHail.ProfileView({
                model: this.model
            });
            this._profile.render();
            this.$("#user-account-container").html(this._profile.el);
            
        },
        
        goToFavorites : function(e){
            e.preventDefault();
        },
        
        goToHistory : function (e) {
            if (e) {
                e.preventDefault();
            }
            var orders = new TaxiHail.OrderCollection();
            orders.fetch({
                url: 'api/account/orders',
                success: _.bind(function (model) {
                    this._history = new TaxiHail.OrderHistoryView({
                        collection:model
                    });
                    this._history.render();
                    this.$("#user-account-container").html(this._history.el);
                }, this)
                
            });
            
            
            
        },
        
        goToPassword : function (e) {
            if (e) {
                e.preventDefault();
            }
            this._password = new TaxiHail.UpdatePasswordView({
                model: this.model
            });
            this._password.render();
            this.$("#user-account-container").html(this._password.el);
        },
        
        selectTab: function ($tab) {
            $tab.addClass('active').siblings().removeClass('active');
        },
        
        ontabclick: function (e) {
            e.preventDefault();

            var $tab = $(e.currentTarget).parent('li');

            this.selectTab($tab);

        },
        
    });

}());
