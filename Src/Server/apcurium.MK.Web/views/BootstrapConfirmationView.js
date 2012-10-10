(function () {

    TaxiHail.BootstrapConfirmationView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=confirm]': 'onConfirm'
        },
        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);
            return this;
        },

        show: function () {
            this.$('.confirmation-modal').modal('show');
        },

        onConfirm: function (e) {
            e.preventDefault();
            this.$('.confirmation-modal').modal('hide');
            this.trigger("ok");
        }
    });

}());