﻿(function () {

    var View = TaxiHail.UpdateTermsAndConditionsView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        events: {
            'click [data-action=savetermsandconditions]': 'savetermsandconditions',
            'click [data-action=retriggertermsandconditions]' : 'retriggertermsandconditions'
        },

        render: function () {
            var data = this.model.toJSON();
            this.$el.html(this.renderTemplate(data));
            return this;
        },

        retriggertermsandconditions: function(e) {
            
            e.preventDefault();
            
            this.model.retrigger()
                .always(_.bind(function() {
                    this.$(':submit').button('reset');
                }, this))
                .done(_.bind(function () {

                    var alert = new TaxiHail.AlertView({
                        message: this.localize('Terms and Conditions Retriggered'),
                        type: 'success'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.message').html(alert.render().el);

                }, this))
                 .fail(_.bind(function () {

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Error Retriggering Terms and Conditions'),
                         type: 'error'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this));
        },

        savetermsandconditions: function (e) {

            e.preventDefault();

            var value = this.$('[name=termsandconditions]').val();

            this.model.save(value)
                 .always(_.bind(function () {

                     this.$(':submit').button('reset');

                 }, this))
                 .done(_.bind(function () {

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Terms and Conditions Saved'),
                         type: 'success'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this))
                 .fail(_.bind(function () {

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Error Saving Terms and Conditions'),
                         type: 'error'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this));

        }
    });
}());