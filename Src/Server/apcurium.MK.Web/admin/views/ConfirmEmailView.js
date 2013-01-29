(function () {
    
    var View = TaxiHail.ConfirmEmailView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',
        
        events: {
            'click [data-action=confirmemail]': 'confirmemail'
        },

        render: function () {
            this.$el.html(this.renderTemplate());

        
        
            return this;
        },
        
        confirmemail: function (e) {
            e.preventDefault();
            var email = this.$('[name=email]').val();
            return $.ajax({
                type: 'PUT',
                url: '../api/account/adminconfirm',
                data: {
                    accountEmail: email
                },
                dataType: 'json',
                success : _.bind(function() {
                    this.$('.errors').text(TaxiHail.localize('confirmEmailSuccess'));
                },this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('confirmEmailError'));
            }),this);
        }

   
    });

    

}());