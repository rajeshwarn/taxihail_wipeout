(function() {

    TaxiHail.UserAccount = Backbone.Model.extend({
        
        url: 'api/account',

        updatePassword: function(currentPassword, newPassword) {
            return $.post('api/accounts/' + this.id + '/updatePassword', {
                currentPassword: currentPassword,
                newPassword: newPassword
            }, function(){}, 'json');
        }
    });

}());