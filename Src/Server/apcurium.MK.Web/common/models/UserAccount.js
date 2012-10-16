(function() {

    TaxiHail.UserAccount = Backbone.Model.extend({
        
        url: TaxiHail.parameters.apiRoot+'/account',

        updatePassword: function(currentPassword, newPassword) {
            return $.post('api/accounts/' + this.id + '/updatePassword', {
                currentPassword: currentPassword,
                newPassword: newPassword
            }, function(){}, 'json');
        },

        updateSettings: function() {
            return $.ajax({
                type: 'PUT',
                url: 'api/account/bookingsettings',
                data: this.get('settings'),
                dataType: 'json'
            });
        }
    });

}());