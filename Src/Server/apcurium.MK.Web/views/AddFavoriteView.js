﻿(function () {

    TaxiHail.AddFavoriteView = TaxiHail.TemplatedView.extend({
        events: {
        },
        render: function () {
            var html = this.renderTemplate();
            this.$el.html(html);
            return this;
        },
    });

}());