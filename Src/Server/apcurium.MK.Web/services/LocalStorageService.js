// Local Storage Service

(function () {

    TaxiHail.store = {
        getItem: function (key) {
            if (localStorage.getItem(key)) {
                return JSON.parse(localStorage.getItem(key));
            }
            return null;
            
        },

        setItem: function (key, value) {
            localStorage.setItem(key, JSON.stringify(value));
        }
    };

}());