(function () {

    TaxiHail.AdminMenuView = TaxiHail.TemplatedView.extend({
       

        events: {
            'click [data-tabname=grantadmin]': 'tograntadmin',
            'click [data-tabname=managefavoritesdefault]': 'tomanagefavoritesdefault'
        },

        render: function() {
            this.$el.html(this.renderTemplate());
            this.$('[data-tabname]').first().addClass('active').siblings().removeClass('active');

            return this;
        },
        
        initialize : function() {
            
        },
        
        tograntadmin : function (e) {
            e.preventDefault();
            this.$('[data-tabname=grantadmin]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('grantadmin', { trigger: true });
        },
        
        tomanagefavoritesdefault: function (e) {
            e.preventDefault();
            this.$('[data-tabname=managefavoritesdefault]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('', { trigger: true });
        },
        
        setActive : function (tabName) {
            this.$('[data-tabname='+tabName+']').addClass('active').siblings().removeClass('active');
        }
    });
}());