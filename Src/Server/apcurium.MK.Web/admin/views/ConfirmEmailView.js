(function () {
    
    var View = TaxiHail.ConfirmEmailView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',
        
        events: {
            'click [data-action=confirmemail]': 'confirmemail',
            'click [data-action=disableemail]': 'disableemail',
            'click [data-action=unlinkaccount]': 'unlinkaccount'
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
                url: '../api/account/adminenable',
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
        },
        
        disableemail: function (e) {
            e.preventDefault();
            var email = this.$('[name=email]').val();
            return $.ajax({
                type: 'PUT',
                url: '../api/account/admindisable',
                data: {
                    accountEmail: email
                },
                dataType: 'json',
                success : _.bind(function() {
                    this.$('.errors').text(TaxiHail.localize('disableEmailSuccess'));
                },this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('disableEmailError'));
            }),this);
        },

        unlinkaccount: function (e) {
            e.preventDefault();
            var email = this.$('[name=email]').val();
            return $.ajax({
                type: 'PUT',
                url: '../api/account/unlink',
                data: {
                    accountEmail: email
                },
                dataType: 'json',
                success: _.bind(function () {
                    this.$('.errors').text(TaxiHail.localize('unlinkAccountSuccess'));
                }, this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('unlinkAccountError'));
            }), this);
        }
    });
}());