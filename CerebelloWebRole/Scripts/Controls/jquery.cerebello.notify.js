(function ($) {
    
    // The actual plugin
    $.notify = function (content, cssClass, timeout) {
        /// <summary>Shows a client-side content</summary>
        /// <param name="content" type="Object">May be plain text or a jQuery object</param>
        /// <param name="cssClass" type="String">Custom CSS class</param>
        /// <param name="timeout" type="Number">Time until it will close automatically</param>

        function closeNotification($notification) {
            $notification.fadeOut("fast", function() {
                $notification.remove();
            });
        }

        // if there's no notifications-wrapper, create it
        var $notificationsWrapper = $(".notifications-wrapper");
        if (!$notificationsWrapper.length)
            $notificationsWrapper = $("<div/>").addClass("notifications-wrapper").appendTo($("body"));

        // creates the DOM elements for the new notification
        var $notification = $("<div/>").addClass("notification").css("display", "none").appendTo($notificationsWrapper);
        if (cssClass)
            $notification.addClass(cssClass);
        $("<div/>").addClass("close").appendTo($notification).click(function () {
            closeNotification($notification);
        });
        $("<div/>").addClass("notification-content").html(content).appendTo($notification);

        if (timeout)
            setTimeout(function() {
                closeNotification($notification);
            }, timeout);

        $notification.fadeIn("fast");
    };

})(jQuery);
