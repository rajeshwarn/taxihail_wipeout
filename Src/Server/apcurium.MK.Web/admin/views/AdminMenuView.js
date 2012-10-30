(function () {

    TaxiHail.AdminMenuView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-route] a': 'onitemclick'

        },

        render: function() {
            this.$el.html(this.renderTemplate());
            this.$('[data-route]').first().addClass('active').siblings().removeClass('active');

            return this;
        },
        
        onitemclick: function(e) {
            e.preventDefault();
            var $item = $(e.currentTarget).closest('li');
            var route = $item.data().route;
            $item.addClass('active').siblings().removeClass('active');
            
            TaxiHail.app.navigate(route, { trigger: true });
        },
        
        tomanagefavoritesdefault: function (e) {
            e.preventDefault();
            this.$('[data-tabname=managefavoritesdefault]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('', { trigger: true });
        },
        
        tomanagepopularaddresses : function (e) {
            e.preventDefault();
            this.$('[data-tabname=managepopularaddresses]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('managepopularaddresses', { trigger: true });
        },
        
        tomanagecompanysettings : function (e) {
            e.preventDefault();
            this.$('[data-tabname=managecompanysettings]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('managecompanysettings', { trigger: true });

        }
    });
}());