function PatientFilesEdit(args) {
    $(function () {
        'use strict';

        // Initialize the jQuery File Upload widget:
        var $fileupload = $('#' + args.FileUploadId);
        $fileupload.fileupload();

        $fileupload.fileupload('option', {
            url: args.FileUploadUrl,
            autoUpload: true,
            maxFileSize: 2048000000, // 2GB
            //resizeMaxWidth: 1920,
            //resizeMaxHeight: 1200,
            dropZone: $fileupload,
            // Callback for file deletion:
            destroy: function (e, data) {
                var that = $(this).data('blueimp-fileupload') ||
                        $(this).data('fileupload');

                var remove = function () {
                    that._transition(data.context).done(
                        function () {
                            $(this).remove();
                            that._adjustMaxNumberOfFiles(1);
                            that._trigger('destroyed', e, data);
                        }
                    );
                };

                if (data.url) {
                    if (data.async) {
                        $.ajax(data);
                        remove();
                    } else {
                        $.ajax(data).done(remove);
                    }
                } else {
                    remove();
                }
            }
        });

        $fileupload.bind('fileuploadfinished', function (e, data) {
            $("input.toggle", data.context).bind('change', function () {
                var $this = $(this);
                if ($this.is(":checked"))
                    $this.parents(".template-download").addClass("selected");
                else
                    $this.parents(".template-download").removeClass("selected");
            });
        });

        $fileupload.fileupload('option', 'done')
            .call($fileupload[0], null, {
                result: args.FileUploadItems
            });

        $('.select-all', $fileupload).bind('click', function (e) {
            e.preventDefault();
            var $chk = $('input.toggle', $fileupload);
            $chk.prop('checked', true);
            $chk.trigger('change');
        });

        $('.unselect-all', $fileupload).bind('click', function (e) {
            e.preventDefault();
            var $chk = $('input.toggle', $fileupload);
            $chk.prop('checked', false);
            $chk.trigger('change');
        });

        var $container = $('#' + args.PanelId);

        $('form', $container).ajaxFileForm({
            success: function (result) {
                $container.replaceWith(result);
            }
        });

        $('.submit-bar a.cancel', $container).click(function (e) {
            e.preventDefault();

            // cancel all pending uploads
            var filesList = $fileupload.fileupload('option', 'filesContainer');
            filesList.find('.cancel').click();

            // applying action to the container
            if (!window.isUnloading) {
                if (args.IsEditing) {
                    $.ajax({ url: args.DetailsUrl, success: function (result) { $container.replaceWith(result); } });
                } else {
                    $container.remove();
                }
            }

            // deleting temporary data from server
            $.ajax({ url: args.DeleteTempFilesUrl, async: !window.isUnloading, complete: function () { } });
        });
    });
}
