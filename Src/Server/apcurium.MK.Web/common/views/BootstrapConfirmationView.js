(function () {

    TaxiHail.BootstrapConfirmationView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=confirm]': 'onConfirm'
        },
        render: function () {
            var html = this.renderTemplate(_.pick(this.options, 'title', 'message', 'confirmButton', 'cancelButton'));
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