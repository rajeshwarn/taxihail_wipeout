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
                        required: true
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
                        required: TaxiHail.localize('Password required')
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

        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            this.model.set($input.attr("name"),  $input.val());
        },
        
        onsubmit: function (form) {
            var currentPassword = $(form).find('[name=password]').val(),
                newPassword = $(form).find('[name=new-password]').val();

            this.model.updatePassword(currentPassword, newPassword)
                .done(_.bind(function(){
                    this.$("#notif-bar").html(TaxiHail.localize('Password updated.'));
                    $(form).find(":text, :password").val('');
                    $(form).find(':submit').button('reset');

                }, this))
                .fail(_.bind(function(response){
                    this.$("#notif-bar").html(TaxiHail.localize(response.statusText));
                }, this));
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());