(function () {

    TaxiHail.AdminMenuView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-route] a': 'onitemclick'

        },

        render: function() {
            this.$el.html(this.renderTemplate({ version: TaxiHail.parameters.version }));
            return this;
        },
        
        onitemclick: function (e) {
              var $item = $(e.currentTarget).closest('li');
            var route = $item.data().route || '';
            
            e.preventDefault();
            $item.addClass('active').siblings().removeClass('active');
            TaxiHail.app.navigate(route, { trigger: true });

        }
    });
}());