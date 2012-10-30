(function () {

    TaxiHail.AdminMenuView = TaxiHail.TemplatedView.extend({
       

        events: {
            'click [data-tabname=grantadmin]': 'tograntadmin',
            'click [data-tabname=managefavoritesdefault]': 'tomanagefavoritesdefault',
            'click [data-tabname=managepopularaddresses]': 'tomanagepopularaddresses',
            'click [data-tabname=managecompanysettings]': 'tomanagecompanysettings'
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
        
        tomanagepopularaddresses : function (e) {
            e.preventDefault();
            this.$('[data-tabname=managepopularaddresses]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('managepopularaddresses', { trigger: true });
        },
        
        tomanagecompanysettings : function (e) {
            e.preventDefault();
            this.$('[data-tabname=managecompanysettings]').addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate('managecompanysettings', { trigger: true });
        },
        
        setActive : function (tabName) {
            this.$('[data-tabname='+tabName+']').addClass('active').siblings().removeClass('active');
        }
    });
}());