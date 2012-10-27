(function ($) {
    
    // The actual plugin
    $.notify = function (content, cssClass, timeout) {

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
