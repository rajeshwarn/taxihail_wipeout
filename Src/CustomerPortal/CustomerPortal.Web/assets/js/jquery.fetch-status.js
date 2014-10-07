(function ($) {

    $.fn.fetchStatus = function () {

        return this.each(function () {
            var $this = $(this),
                id = $this.data().key;
                $.get('/Admin/Deployment/StatusText/'+id, function (result) {                
                                                                                $this.html(result);                                
                });
            
                $.get('/Admin/Deployment/DetailsText/' + id, function (result) {
                    
                    $this.prop('title', result);                    
                });
            
                        
        });
    };
    
    

}(jQuery));