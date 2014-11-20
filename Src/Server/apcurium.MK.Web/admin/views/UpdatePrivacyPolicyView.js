(function () {

    var View = TaxiHail.UpdatePrivacyPolicyView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        events: {
            'click [data-action=saveprivacypolicy]': 'saveprivacypolicy'
        },

        render: function () {
            var data = this.model.toJSON();
            this.$el.html(this.renderTemplate(data));
            return this;
        },

        saveprivacypolicy: function (e) {

            e.preventDefault();

            var value = this.$('[name=privacypolicy]').val();

            this.model.save(value)
                 .always(_.bind(function () {

                     this.$(':submit').button('reset');

                 }, this))
                 .done(_.bind(function () {

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Privacy Policy Saved'),
                         type: 'success'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this))
                 .fail(_.bind(function () {

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Error Saving Privacy Policy'),
                         type: 'error'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this));

        }
    });
}());