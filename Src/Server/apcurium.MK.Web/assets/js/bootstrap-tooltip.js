// This extends Bootstrap Tooltip to support offsetX and offsetY options for custom placement
!function ($) {

  "use strict"; // jshint ;_;

  $.fn.tooltip.Constructor.prototype.show = function () {
    var $tip
      , inside
      , pos
      , actualWidth
      , actualHeight
      , offsetX
      , offsetY
      , placement
      , tp

    if (this.hasContent() && this.enabled) {
      $tip = this.tip()
      this.setContent()

      if (this.options.animation) {
        $tip.addClass('fade')
      }

      placement = typeof this.options.placement == 'function' ?
        this.options.placement.call(this, $tip[0], this.$element[0]) :
        this.options.placement

      inside = /in/.test(placement)

      offsetX = typeof this.options.offsetX == 'number' ? this.options.offsetX : 0 
      offsetY = typeof this.options.offsetY == 'number' ? this.options.offsetY : 0 

      $tip
        .remove()
        .css({ top: 0, left: 0, display: 'block' })
        .appendTo(inside ? this.$element : document.body)

      pos = this.getPosition(inside)

      actualWidth = $tip[0].offsetWidth
      actualHeight = $tip[0].offsetHeight

      switch (inside ? placement.split(' ')[1] : placement) {
        case 'bottom':
          tp = {top: pos.top + pos.height + offsetY, left: pos.left + pos.width / 2 - actualWidth / 2 + offsetX}
          break
        case 'top':
          tp = {top: pos.top - actualHeight + offsetY, left: pos.left + pos.width / 2 - actualWidth / 2 + offsetX}
          break
        case 'left':
          tp = {top: pos.top + pos.height / 2 - actualHeight / 2 + offsetY, left: pos.left - actualWidth + offsetX}
          break
        case 'right':
          tp = {top: pos.top + pos.height / 2 - actualHeight / 2 + offsetY, left: pos.left + pos.width + offsetX}
          break
      }

      $tip
        .css(tp)
        .addClass(placement)
        .addClass('in')
    }
  };

}(window.jQuery);
