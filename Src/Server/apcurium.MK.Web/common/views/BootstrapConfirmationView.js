(function () {

    TaxiHail.BootstrapConfirmationView = TaxiHail.TemplatedView.extend({
        options: {
            autoHide: true
        },

        events: {
            'click [data-action=confirm]': 'onConfirm',
            'click [data-dismiss=modal]': 'onCancel'
        },
        render: function () {
            var html = this.renderTemplate(_.pick(this.options, 'title', 'message', 'confirmButton', 'cancelButton'));
            this.$el.html(html);
            return this;
        },

        show: function () {
            this.$('.confirmation-modal').modal('show');
        },

        hide: function () {
            this.$('.confirmation-modal').modal('hide');
        },

        onConfirm: function (e) {
            e.preventDefault();
            this.options.autoHide && this.hide();
            this.trigger("ok");
        },

        onCancel: function (e) {
            e.preventDefault();
            this.options.autoHide && this.hide();
            this.trigger("cancel");
        }
    });

}());