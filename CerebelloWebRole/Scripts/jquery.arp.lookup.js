(function ($) {

    //permite ao usuario navegar em uma lista grande de opções
    function Lookup(el, options) {
        var _this = this;

        //Defaults:
        this.defaults = {
            contentUrl: "",
            inputHiddenId: "",
            inputTextId: "",
            inputTextValue: "",
            inputHiddenValue: "",
            searchParamName: 'term',
            pageIndexParamName: 'pageIndex',
            pageSizeParamName: 'pageSize',
            pageSize: 5,
            ajaxParams: {},
            formatItem: function () { },
            onRowSelected: function (id, text) { }
        };

        //Extending options:
        _this.opts = $.extend({}, this.defaults, options);

        //Privates:
        _this.$el = $(el);

        _this.$lookupInput = null;
        _this.$lookupInputHidden = null;
        _this.$lookupFilterButton = null;
        _this.$lookupClearButton = null;
        _this.$lookupDropdownButton = null;
        _this.$lookupPagerWrapper = null;
        _this.$lookupDropdown = null;
        _this.$lookupWrapper = null;

        _this.lookupPaginator = null;
        _this.lookupDropdownId = null;
        _this.lookupInputTextCache = null;
        _this.lookupInputTextDirtyCache = null;

        _this.isDropdownCreated = function () {
            return _this.$lookupDropdown && _this.$lookupDropdown.length > 0;
        };

        _this.isDropdownVisible = function () {
            return _this.isDropdownCreated() && _this.$lookupDropdown.is(":visible");
        };

        _this.recalculatePosition = function () {
            _this.$lookupDropdown.css("left", _this.$lookupInput.offset().left - 26);
            _this.$lookupDropdown.css("top", _this.$lookupInput.offset().top + _this.$lookupInput.outerHeight() + 3);
            _this.$lookupDropdown.css("width", _this.$el.outerWidth() + 100);
        };

        _this.ensureDropdownIsVisible = function () {
            _this.recalculatePosition();
            if (_this.$lookupDropdown && !_this.$lookupDropdown.is(":visible"))
                _this.$lookupDropdown.show();
        };

        _this.createDropdown = function (searchTerm, showDropdown) {
            $("body").append($("<div id='" + _this.lookupDropdownId +
                "' class='lookup-dropdown'><div class='lookup-wrapper'></div><div class='lookup-pager-wrapper'></div></div>"));

            _this.$lookupDropdown = $("#" + _this.lookupDropdownId);
            _this.$lookupDropdown.css("left", _this.$lookupInput.position().left - 26);
            _this.$lookupDropdown.css("top", _this.$lookupInput.position().top + _this.$lookupInput.outerHeight() + 3);
            _this.$lookupDropdown.css("width", _this.$el.outerWidth() + 100);

            _this.$lookupWrapper = $(".lookup-wrapper", _this.$lookupDropdown);
            _this.$lookupPagerWrapper = $(".lookup-pager-wrapper", _this.$lookupDropdown);

            $('html').bind("click", function () {
                _this.$lookupDropdown.hide()
            });

            _this.$lookupDropdown.bind("click", function (e) {
                e.stopPropagation();
                _this.$lookupInput.focus();
            });

            _this.getData(searchTerm, undefined, undefined, showDropdown);
        };

        _this.moveFocusForward = function () {
            var selected = $("tr.selected", _this.$lookupWrapper);
            // se já existe uma linha selecionada
            if (selected.length) {
                var next = selected.next("tr");
                if (next.length)
                    next.addClass("selected").siblings().removeClass("selected");
                else
                    _this.lookupPaginator.nextPage(function () {
                        var firstTr = $("tr", _this.$lookupWrapper).filter(":first");
                        if (firstTr.length)
                            firstTr.addClass("selected").siblings().removeClass("selected");
                    });
            }
            else {
                var firstTr = $("tr", _this.$lookupWrapper).eq(0);
                if (firstTr.length)
                    firstTr.addClass("selected").siblings().removeClass("selected");
            }
        };

        _this.moveFocusBackward = function () {
            var selected = $("tr.selected", _this.$lookupWrapper);
            if (selected.length) {
                var prev = selected.prev("tr");
                if (prev.length)
                    prev.addClass("selected").siblings().removeClass("selected");
                else if (_this.lookupPaginator.prevPage(function () {
                    var lastTr = $("tr", _this.$lookupWrapper).filter(":last");
                    if (lastTr.length)
                        lastTr.addClass("selected").siblings().removeClass("selected");
                })) {

                }
                else
                    $("tr", _this.$lookupWrapper).removeClass("selected");
            }
        };

        _this.selectCurrentlyFocusedRow = function () {
            var selectedRow = $("tr.selected", _this.$lookupWrapper);
            var selectedText = $("td", selectedRow).html();
            var selectedId = selectedRow.attr("data-val-id");
            _this.lookupInputTextCache = selectedText;
            _this.lookupInputTextDirtyCache = selectedText;
            _this.$lookupInput.val(selectedText);
            _this.$lookupInputHidden.val(selectedId);
            _this.$lookupDropdown.hide();
            _this.opts.onRowSelected(selectedId, selectedText);
        };

        _this.filter = function (showDropdown) {
            var searchTerm = _this.$lookupInput.val();
            if (!_this.isDropdownCreated())
                _this.createDropdown(searchTerm, showDropdown);
            else
                _this.getData(searchTerm, undefined, undefined, showDropdown);
        };

        _this.clear = function () {
            _this.$lookupInput.val('');
            _this.$lookupInputHidden.val('');
            _this.lookupInputTextCache = null;
            _this.lookupInputTextDirtyCache = null;
            $("tr", _this.$lookupWrapper).removeClass("selected");
            _this.opts.onRowSelected(undefined, undefined);
        };

        _this.getData = function (searchTerm, pageIndex, onDataReceived, showDropdown) {
            if (showDropdown == undefined)
                showDropdown = true;

            _this.$el.addClass("loading");

            var params = new Object();
            $.extend(params, _this.opts.ajaxParams);

            if (searchTerm)
                params[_this.opts.searchParamName] = searchTerm;
            if (pageIndex)
                params[_this.opts.pageIndexParamName] = pageIndex;

            params[_this.opts.pageSizeParamName] = _this.opts.pageSize;

            $.ajax({
                url: _this.opts.contentUrl,
                data: params,
                dataType: 'json',
                success: function (data) {
                    // vejo se existe alguém selecionado, porque se existir,
                    // eu vou selecionar o primeiro elemento desta nova 
                    // sessão de dados
                    var selected = $("tr.selected", _this.$lookupWrapper);

                    // criar a tabela
                    var rows = [];

                    $.each(data.Rows, function (key, val) {
                        rows.push("<tr data-val-id='" + val.Id + "'><td>" + val.Value + "</td></tr>");
                    });

                    if (rows.length > 0)
                        _this.$lookupWrapper.html($("<table />", {
                            html: rows.join('')
                        }));
                    else
                        _this.$lookupWrapper.html("<div class='empty-box'>A pesquisa não retornou registros</div>");

                    $("tr", _this.$lookupWrapper).bind("click", function () {
                        $(this).addClass("selected").siblings().removeClass("selected");
                        _this.selectCurrentlyFocusedRow();
                    });

                    // aplicando o pager em lookupPagerWrapper
                    _this.lookupPaginator = _this.$lookupPagerWrapper.pager({
                        count: data.Count,
                        rowsPerPage: _this.opts.pageSize,
                        currentPageIndex: pageIndex,
                        onPageChanged: function (i, onDataReceived) {
                            _this.getData(searchTerm, i, onDataReceived);
                        }
                    }).data('pager');

                    if (onDataReceived)
                        onDataReceived();

                    if (showDropdown)
                        _this.ensureDropdownIsVisible();

                    _this.$el.removeClass("loading");
                }
            });
        };
    }

    // Separate functionality from object creation
    Lookup.prototype = {

        init: function () {
            var _this = this;

            _this.$el.append("<div class='lookup-search-box'><div class='buttons-wrapper'><div class='lookup-filter'></div><div class='lookup-clear-search'></div><div class='lookup-dropdown-button'></div></div><div class='input-wrapper'><input type='text' id='" + _this.opts.inputTextId +
                "' name='" + _this.opts.inputTextId + "' class='search ac_input' autocomplete='off' /><input type='hidden' id='" +
                _this.opts.inputHiddenId + "' name='" + _this.opts.inputHiddenId +
                "' /></div></div>");

            _this.$lookupInput = $("input[type='text']", _this.$el);
            _this.$lookupInputHidden = $("input[type='hidden']", _this.$el);
            _this.$lookupFilterButton = $(".lookup-filter", _this.$el);
            _this.$lookupClearButton = $(".lookup-clear-search", _this.$el);
            _this.$lookupDropdownButton = $(".lookup-dropdown-button", _this.$el);
            _this.lookupDropdownId = _this.$el.attr("id") + "-dropdown";

            _this.$lookupInput.val(_this.opts.inputTextValue);
            _this.lookupInputTextCache = _this.opts.inputTextValue;
            _this.lookupInputTextDirtyCache = _this.opts.inputTextValue;
            _this.$lookupInputHidden.val(_this.opts.inputHiddenValue);

            _this.$lookupClearButton.bind("click", function (e) {
                _this.lookupInputTextDirtyCache = null;
                _this.clear();
                _this.$lookupInput.focus();
            });

            _this.$lookupFilterButton.bind("click", function (e) {
                _this.$lookupInput.val(_this.lookupInputTextDirtyCache);
                _this.filter();
                _this.$lookupInput.focus();
            });

            _this.$lookupDropdownButton.bind("click", function (e) {
                e.stopPropagation();
                _this.$lookupInput.val(_this.lookupInputTextDirtyCache);
                if (!_this.isDropdownVisible()) {
                    if (!_this.isDropdownCreated())
                        _this.createDropdown();
                    _this.ensureDropdownIsVisible();
                }
                else
                    _this.$lookupDropdown.hide();
                _this.$lookupInput.focus();
            });

            _this.$lookupInput.bind("blur", function (e) {
                if (_this.$lookupInput.val() != _this.lookupInputTextCache) {
                    _this.$lookupInputHidden.val('');
                    _this.$el.val('');
                    $("tr", _this.$lookupWrapper).removeClass("selected");
                }
            });

            _this.$lookupInput.bind("keyup", function (e) {
                _this.lookupInputTextDirtyCache = _this.$lookupInput.val();
            });

            _this.$lookupInput.bind("keydown", function (e) {
                switch (e.keyCode) {
                    // enter                                                                                                                                                     
                    case 13:
                        e.preventDefault();
                        if (_this.isDropdownCreated() && !_this.isDropdownVisible()) {
                            var searchTerm = _this.$lookupInput.val();
                            _this.getData(searchTerm);
                            _this.ensureDropdownIsVisible();
                        } else {
                            // selection
                            if (_this.isDropdownCreated() && $("tr.selected", _this.$lookupWrapper).length) {
                                _this.selectCurrentlyFocusedRow();
                            }
                            // filter
                            else {
                                _this.filter();
                            }
                        }
                        break;
                    // seta pra baixo                                                                                                  
                    case 40:
                        if (!_this.isDropdownCreated())
                            _this.createDropdown();
                        else {
                            _this.moveFocusForward();
                            _this.ensureDropdownIsVisible();
                        }
                        break;
                    // seta para cima                                                                                                                                              
                    case 38:
                        if (_this.isDropdownCreated()) {
                            _this.moveFocusBackward();
                            _this.ensureDropdownIsVisible();
                        }
                        break;
                    default:
                        _this.$lookupInputHidden.val('');
                        $("tr", _this.$lookupWrapper).removeClass("selected");
                        break;
                }
            });
        }
    };

    // The actual plugin
    $.fn.lookup = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new Lookup(this, options);
                rev.init();
                $(this).data('lookup', rev);
            });
        }
        return this;
    };
})(jQuery);