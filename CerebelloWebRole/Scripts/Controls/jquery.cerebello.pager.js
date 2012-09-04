(function ($) {

    // provides client-side paging
    function Pager(el, options) {

        // Defaults:
        this.defaults = {
            count: 0,
            rowsPerPage: 20,
            currentPageIndex: 1,
            enabled: true,
            onPageChanged: function () { }
        };

        if (options.currentPageIndex == null)
            options.currentPageIndex = undefined;

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);

        this.$prevButton = null;
        this.$nextButton = null;
        this.$currentPage = null;
        this.$pageCount = null;
        this.handleButtons = function () { };
        this.pageCount = 0;
    }

    // Separate functionality from object creation
    Pager.prototype = {

        init: function () {
            var _this = this;

            _this.$el.addClass("paginator");
            _this.pageCount = Math.ceil(_this.opts.count / _this.opts.rowsPerPage);

            if (_this.pageCount < 2) {
                _this.$el.hide();
            }
            else {
                _this.$el.show();
                if (_this.opts.currentPageIndex > _this.pageCount)
                    _this.opts.currentPageIndex = _this.pageCount;
            }

            _this.$el.html("<a href='#' class='paginator-prev'>anterior</a><span class='paginator-current-page'>" +
                _this.opts.currentPageIndex + "</span>/<span class='paginator-page-count'>" +
                _this.pageCount +
                "</span><a href='#' class='paginator-next'>próxima</a>");

            this.$prevButton = $(".paginator-prev", _this.$el);
            this.$nextButton = $(".paginator-next", _this.$el);
            this.$currentPage = $(".paginator-current-page", _this.$el);
            this.$pageCount = $(".paginator-page-count", _this.$el);

            this.$prevButton.bind("click", function (e) {
                e.preventDefault();
                if(_this.opts.enabled)
                    _this.prevPage();
            });

            this.$nextButton.bind("click", function (e) {
                e.preventDefault();
                if (_this.opts.enabled)
                    _this.nextPage();
            });

            _this.handleButtons();
        },

        //move o paginador para a próxima página
        nextPage: function (onDataReceived) {
            var _this = this;
            if (_this.opts.currentPageIndex < _this.pageCount) {
                _this.opts.currentPageIndex++;
                _this.$currentPage.html(_this.opts.currentPageIndex);
                _this.handleButtons();
                _this.opts.onPageChanged(_this.opts.currentPageIndex, onDataReceived);
                return true;
            }
            return false;
        },

        //move o paginador para a página anterior
        prevPage: function (onDataReceived) {
            var _this = this;
            if (_this.opts.currentPageIndex > 1) {
                _this.opts.currentPageIndex--;
                _this.$currentPage.html(_this.opts.currentPageIndex);
                _this.handleButtons();
                _this.opts.onPageChanged(_this.opts.currentPageIndex, onDataReceived);
                return true;
            }
            return false;
        }
    };

    // The actual plugin
    $.fn.pager = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new Pager(this, options);
                rev.init();
                $(this).data('pager', rev);
            });
        }
        return this;
    };
})(jQuery);
