(function ($) {
    // The actual plugin
    $.fn.setCaretPosition = function (position) {
        if (this.length) {
            this.each(function () {
                if (this.createTextRange) {
                    var range = this.createTextRange();
                    range.move('character', position);
                    range.select();
                }
                else {
                    this.focus();
                    this.setSelectionRange(position, position);
                }
            });
        }
        return this;
    };
})(jQuery);