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
            e.preventDefault();
        },
        
        goToPassword : function (e) {
            e.preventDefault();
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
