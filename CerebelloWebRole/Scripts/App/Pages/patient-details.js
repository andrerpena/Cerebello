$(function () {
    'use strict';

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
});
