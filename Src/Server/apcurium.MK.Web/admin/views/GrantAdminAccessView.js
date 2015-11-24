(function () {
    
    var View = TaxiHail.GrantAdminAccessView = TaxiHail.TemplatedView.extend({

        tagName: 'div',
        className: 'form-horizontal',
        
        events: {
            'click [data-action^=grant]': 'grantadmin',
            'click [data-action^=revoke]': 'grantadmin'
        },
        
        render: function () {
            this.$el.html(this.renderTemplate(TaxiHail.parameters));

            return this;
        },
        
        grantadmin: function (e) {
            e.preventDefault();
            var action = $(e.currentTarget).data().action,
                email = this.$('[name=email]').val();
            
            return $.ajax({
                type: 'PUT',
                url: '../api/account/' + action,
                data: {
                    accountEmail: email
                },
                dataType: 'json',
                success : _.bind(function() {
                    this.$('.errors').text(TaxiHail.localize(action + 'Success'));
                },this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('grantAccessError'));
            }),this);
        }
    });

    

}());