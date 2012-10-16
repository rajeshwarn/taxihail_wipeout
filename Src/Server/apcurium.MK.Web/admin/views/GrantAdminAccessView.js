(function () {
    
    var View = TaxiHail.GrantAdminAccessView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',
        
        events: {
            'click [data-action=grantadmin]': 'grantadmin'
        },

        render: function () {
            this.$el.html(this.renderTemplate());

        
        
            return this;
        },
        
        grantadmin: function (e) {
            e.preventDefault();
            var email = this.$('.email').val();
            return $.ajax({
                type: 'PUT',
                url: '../api/account/grantadmin',
                data: {
                    accountEmail: email
                },
                dataType: 'json',
                success : _.bind(function() {
                    this.$('.errors').text(TaxiHail.localize('grantAdminSuccess'));
            },this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('grantAdminError'));
            }
                ),this);
                /*.error(function(e) {
                    this.$('.notification-zone').text('error ! check that mail is correct' +e );
                }
                );*/
        }

   
    });

    

}());