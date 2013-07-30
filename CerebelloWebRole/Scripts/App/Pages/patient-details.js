$(function () {
    'use strict';

    $("#alter-picture").click(function (e) {
        var $this = $(this);
        e.preventDefault();
        $.cameraWindow({
            success: function (data) {
                $.ajax($this.attr("data-UrlTransferPicture"),
                    {
                        data: {
                            tempFileName: data.fileName
                        },
                        success: function () {
                            var originalSrc = $("#patient-picture").attr("data-val-original-src");
                            $("#patient-picture").attr("src", originalSrc + "&foo=" + new Date().getTime());
                        },
                        error: function () {

                        }
                    });
            }
        });
    });

    $("#remove-picture").click(function (e) {
        var $this = $(this);
        e.preventDefault();
        if (confirm("Deseja realmente excluir a foto do paciente? esta operação não pode ser desfeita")) {
            $.ajax($this.attr("data-UrlDeletePicture"),
                {
                    success: function () {
                        var originalSrc = $("#patient-picture").attr("data-val-original-src");
                        $("#patient-picture").attr("src", originalSrc + "&foo=" + new Date().getTime());
                    },
                    error: function () {

                    }
                });
        };
    });

    $(window).bind("beforeunload", function (e) {
        var $preventNav = $(".prevent-nav");
        if ($preventNav.length > 0) {
            return "Existem dados não salvos nesta tela, se você prosseguir eles serão perdidos. Gostaria de prosseguir mesmo assim?";
        }
    });

    $(window).bind("unload", function (e) {
        window.isUnloading = true;
        $(".nav-away").trigger("click");
    });

    // Preventing browser default behavior when drag and dropping.
    $(document).bind('drop dragover', function (e) {
        e.preventDefault();
    });

    $(document).bind('dragover', function (e) {
        var dropZone = $('.dropzone'),
            timeout = window.dropZoneTimeout;

        dropZone.addClass('fade');

        if (!timeout) {
            dropZone.addClass('in');
        } else {
            clearTimeout(timeout);
        }

        dropZone.each(function (name, value) {
            if (e.target === value || jQuery.contains(value, e.target)) {
                $(value).addClass('hover');
            } else {
                $(value).removeClass('hover');
            }
        });

        window.dropZoneTimeout = setTimeout(function () {
            dropZone.removeClass('in hover');
            window.dropZoneTimeout = null;
        }, 100);
    });

    $("#schedule-appointment").click(function (e) {
        var $this = $(this);
        e.preventDefault();
        $.modal({
            url: $this.attr("href"),
            title: "Nova consulta",
            width: 480,
            height: 200
        });
    });

    $("a.add-appointment-panel").click(function (e) {
        e.preventDefault();
        var $this = $(this);
        $this.removeClass("icon-link-plus").addClass("icon-link-loading");
        $.ajax({
            url: $this.attr("href"),
            cache: false,
            success: function (html) {
                var $appointmentPanel = $this.siblings(".appointment-panel-wrapper");
                $appointmentPanel.show();
                $appointmentPanel.prepend(html);
                $appointmentPanel.bind("cancel", function () {
                    if ($appointmentPanel.children().length == 0)
                        $appointmentPanel.hide();
                });
                $this.removeClass("icon-link-loading").addClass("icon-link-plus");
            },
            error: function () {
                $this.removeClass("icon-link-loading").addClass("icon-link-plus");
            }
        });
    });

    $("#delete-patient-link").click(function (e) {
        var $this = $(this);
        e.preventDefault();
        $.deleteConfirmation({
            objectType: "paciente",
            objectName: $this.attr("data-objectName"),
            url: $this.attr("data-deleteUrl"),
            success: function () {
                alert("Este paciente foi excluído");
                window.location = $this.attr("data-deleteRedirect");
            }
        });
    });
});
