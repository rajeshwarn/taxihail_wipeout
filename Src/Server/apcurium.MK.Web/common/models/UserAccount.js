(function() {

    TaxiHail.UserAccount = Backbone.Model.extend({
        
        url: TaxiHail.parameters.apiRoot+'/account',

        updatePassword: function(currentPassword, newPassword) {
            return $.post('api/accounts/' + this.id + '/updatePassword', {
                currentPassword: currentPassword,
                newPassword: newPassword
            }, function(){}, 'json');
        },

        updateSettings: function () {
            var settings = this.get('settings');

            return $.ajax({
                type: 'PUT',
                url: 'api/account/bookingsettings',
                data: settings,
                dataType: 'json'
            });
        },

        getChargeAccount: function (accountNumber) {
            return $.get('api/admin/accountscharge/' + accountNumber + '/true', function () { }, 'json');;
        }
    });

}());