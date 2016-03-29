(function() {

    TaxiHail.UserAccount = Backbone.Model.extend({
        
        url: TaxiHail.parameters.apiRoot+'/accounts',

        updatePassword: function(currentPassword, newPassword) {
            return $.post(TaxiHail.parameters.apiRoot + '/accounts/' + this.id + '/updatePassword', {
                currentPassword: currentPassword,
                newPassword: newPassword
            }, function(){}, 'json');
        },

        updateSettings: function () {
            var settings = this.get('settings');

            var result = $.ajax({
                type: 'PUT',
                url: TaxiHail.parameters.apiRoot + '/accounts/bookingsettings',
                data: JSON.stringify(settings),
                dataType: 'json',
                contentType: 'application/json; charset=UTF-8'
            });

            return result;
        },

        getChargeAccount: function (accountNumber, customerNumber) {
            return $.get(TaxiHail.parameters.apiRoot + '/admin/accountscharge/' + accountNumber + '/' + customerNumber + '/true', function () { }, 'json');;
        }
    });

}());