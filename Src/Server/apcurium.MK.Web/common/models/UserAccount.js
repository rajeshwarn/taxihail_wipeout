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

            var result = $.ajax({
                type: 'PUT',
                url: 'api/account/bookingsettings',
                data: JSON.stringify(settings),
                dataType: 'json',
                contentType: 'application/json; charset=UTF-8'
            });

            return result;
        },

        getChargeAccount: function (accountNumber, customerNumber) {
            return $.get('api/admin/accountscharge/' + accountNumber + '/' + customerNumber + '/true', function () { }, 'json');;
        }
    });

}());