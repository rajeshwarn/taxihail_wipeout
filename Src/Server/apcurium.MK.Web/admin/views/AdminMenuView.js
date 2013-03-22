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
        
        onitemclick: function (e) {
            var $item = $(e.currentTarget).closest('li');
            var route = $item.data().route;
            
            if (route) {
                e.preventDefault();
                $item.addClass('active').siblings().removeClass('active');
                TaxiHail.app.navigate(route, { trigger: true });
            }
            else {
                $item.addClass('active').siblings().removeClass('active');

            }
        }
    });
}());