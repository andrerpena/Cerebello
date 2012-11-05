(function ($) {

    // Allows for creating a dropdown-menu out of any element in the page. Anything can become a dropdown menu.
    function DropdownMenu(el, options) {

        // Defaults:
        this.defaults = {
            // items here must be of the format
            // {id: "myId", text: "my text", href: "http:\\www.cerebello.com.br" }
            // OR 
            // {id: "myId", text: "my text" } <- in this case, you can handle the click in the onItemClicked event
            items: new Array(),
            onItemClicked: function (itemId) { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }

    // Separate functionality from object creation
    DropdownMenu.prototype = {

        init: function () {
            var _this = this;

            // creates a new balloon for this dropdown
            var $balloon = $("<div/>").addClass("balloon").appendTo($(window.document.body));
            var $balloonPointer = $("<div/>").addClass("balloon-pointer").appendTo($balloon);

            // adding the arrow divs to the pointer
            $balloonPointer.append($("<div/>").addClass("balloon-arrow menu-target"));
            $balloonPointer.append($("<div/>").addClass("balloon-arrow-border"));

            var $balloonWrapper = $("<div/>").addClass("balloon-wrapper").appendTo($balloon);
            var $balloonContent = $("<div/>").addClass("balloon-content").appendTo($balloonWrapper);

            var $dropdownList = $("<ul/>").addClass("balloon-links").appendTo($balloonContent);

            for (var i = 0; i < _this.opts.items.length; i++) {
                var $li = $("<li/>").appendTo($dropdownList);
                var $a = $("<a/>").appendTo($li);
                $a.attr("data-val-item-id", _this.opts.items[i].id);
                $a.text(_this.opts.items[i].text);
                if (_this.opts.items[i].href)
                    $a.attr("href", _this.opts.items[i].href);
            };

            $("a", $dropdownList).bind("click", function (e) {
                _this.opts.onItemClicked($(e.target).attr("data-val-item-id"));
            });

            _this.$el.bind("click", function (e) {
                e.stopPropagation();
                $balloon.css("left", _this.$el.offset().left);
                $balloon.css("top", _this.$el.offset().top + _this.$el.outerHeight() + 5);
                $balloon.show();
                var handler = function (e2) {
                    if (!$balloon.has(e2.target).length) {
                        $balloon.hide();
                        $("html").unbind("click", handler);
                    }
                };
                $("html").click("click", handler);
            });

            $(window).resize(function () {
                $balloon.hide();
            });
        }
    };

    // The actual plugin
    $.fn.dropdownMenu = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new DropdownMenu(this, options);
                rev.init();
                $(this).data('dropdownMenu', rev);
            });
        }
        return this;
    };
})(jQuery);
