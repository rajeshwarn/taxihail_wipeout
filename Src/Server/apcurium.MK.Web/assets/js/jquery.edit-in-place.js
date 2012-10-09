(function( $ ){

    $.fn.editInPlace = function( options ) {  

        return this.filter(':text, select').each(function() {        

            var $element = $(this),
                $label = $('<span>')
                    .data('element', this)
                    .addClass('control-label')
                    .css({
                        display: 'inline-block',
                        width: $element.width()
                    })
                    .on('click', function() {
                        $($(this).data().element).show();
                        $(this).remove();
                    });

            if($element.is(':text')) {
                var val = $element.val() || $element.attr('placeholder');
                $label.text(val);
            } else if($element.is('select')) {
                $label.text($element.find(':selected').text());
            }

            $label.insertAfter($element.hide());
        });

    };
})( jQuery );