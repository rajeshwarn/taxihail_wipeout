(function () {
    var settingschanged = false;
    
    var View = TaxiHail.UpdatePasswordView = TaxiHail.TemplatedView.extend({
        
        tagName: 'form',
        className: 'form-horizontal',

        initialize: function() {
            _.bindAll(this, 'onsubmit');
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            
            this.validate({
                rules: {
                    password: "required",
                    'new-password': {
                        required: true,
                        minlength: 6
                },
                    'confirm-password': {
                        required: true,
                        equalTo: "#newPassword"
                    }
                },
                messages: {
                    password : {
                        required : TaxiHail.localize('Password required')
                    },
                    'new-password' : {
                        required: TaxiHail.localize('Password required'),
                        minlength: TaxiHail.localize('Password length')
                    },
                    'confirm-password': {
                        required: TaxiHail.localize('Password required'),
                        equalTo: TaxiHail.localize('Password are not the same')
                    }
                },
                submitHandler: this.onsubmit
            });

            return this;
        },

        renderConfirmationMessage: function() {
            var view = new TaxiHail.AlertView({
                message: TaxiHail.localize('Password updated.'),
                type: 'success'
            });
            view.on('ok', this.render, this);
            this.$el.html(view.render().el);
        },

        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            this.model.set($input.attr("name"),  $input.val());
        },
        
        onsubmit: function (form) {
            var currentPassword = $(form).find('[name=password]').val(),
                newPassword = $(form).find('[name=new-password]').val();

            this.model.updatePassword(currentPassword, newPassword)
                .done(_.bind(function() {

                    this.renderConfirmationMessage();

                }, this))
                .fail(_.bind(function(response){

                    this.$(':submit').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(response.statusText),
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);

                }, this));
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());