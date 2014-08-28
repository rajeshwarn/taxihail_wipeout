(function () {

    var View = TaxiHail.ManageNotificationSettingsView = TaxiHail.TemplatedView.extend({
        tagName: 'form',
        className: 'well clearfix form-horizontal',

        render: function () {
            
            var data = this.model.toJSON();
            
            var sum = _.reduce(data, function (memo, value, key) {
                memo.push({ key: key, value: value });
                return memo;
            }, []);

            this.$el.html(this.renderTemplate({ settings: sum }));
            
            this.validate({
                submitHandler: this.save
            });

            return this;
        },

        save: function (form) {
            
            var data = this.serializeForm(form);

            this.model.batchSave(data)
                 .always(_.bind(function() {

                     this.$(':submit').button('reset');

                 }, this))
                 .done(_.bind(function(){

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Settings Saved'),
                         type: 'success'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this))
                 .fail(_.bind(function(){

                     var alert = new TaxiHail.AlertView({
                         message: this.localize('Error Saving Settings'),
                         type: 'error'
                     });
                     alert.on('ok', alert.remove, alert);
                     this.$('.message').html(alert.render().el);

                 }, this));
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());