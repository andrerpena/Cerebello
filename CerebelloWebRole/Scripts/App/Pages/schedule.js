$(function (e) {

    window.datesWithAppointments = new Array();
    window.fetchDatePickerEvents = function (year, month) {
        if (!year || !month) {
            var currentTime = $(".datepicker").datepicker("getDate");
            year = currentTime.getFullYear();
            month = currentTime.getMonth() + 1;
        }
        $.ajax({
            url: window.getDatesWithAppointmentsUrl,
            data: { year: year, month: month },
            success: function (result) {
                window.datesWithAppointments = result;
                $(".datepicker").datepicker("refresh");
            }
        });
    };

    $(".datepicker").datepicker({
        showOtherMonths: false,
        selectOtherMonths: false,
        onSelect: function (dateText, inst) {
            var match = dateText.match(/(\d{2})\/(\d{2})\/(\d{4})/);
            $('#calendar').fullCalendar('gotoDate', parseInt(match[3], 10), parseInt(match[2], 10) - 1, parseInt(match[1], 10));
        },
        beforeShowDay: function (date) {
            return [
                // indicates that the date is selectable
                true,
                // sets a css class to this date item
                jQuery.inArray(
                    $.datepicker.formatDate("'d'dd_mm_yy", date),
                    window.datesWithAppointments
                ) != -1 ? "dateWithAppointments" : ""
            ];
        },
        onChangeMonthYear: function (year, month, inst) {
            fetchDatePickerEvents(year, month);
        }
    });


    $('#calendar').fullCalendar({
        // this is how you disable the vertical scroll. Ugly but this is the way.
        height: 999999999,
        header: { left: 'agendaWeek,agendaDay', center: '', right: 'today prev,next' },
        aspectRatio: 1,
        events: window.eventsUrl,
        weekends: window.weekends,
        selectable: true,
        defaultView: 'agendaDay',
        // translation
        columnFormat: {
            month: 'ddd',    // Mon
            week: 'ddd d/M', // Mon 9/7
            day: 'dddd d/M'  // Monday 9/7
        },
        buttonText: {
            prev: '&nbsp;&#9668;&nbsp;',  // left triangle
            next: '&nbsp;&#9658;&nbsp;',  // right triangle
            prevYear: '&nbsp;&lt;&lt;&nbsp;', // <<
            nextYear: '&nbsp;&gt;&gt;&nbsp;', // >>
        },
        allDaySlot: false,
        axisFormat: "HH:mm",
        timeFormat: "HH:mm{ - HH:mm}",
        slotMinutes: window.slotMinutes,
        minTime: window.minTime,
        maxTime: window.maxTime,
//        eventAfterRender: function (event, element) {
//            $(element).find(".fc-event-head").removeClass("fc-event-skin");

//            var eventTime = $(element).find(".fc-event-time");
//            eventTime.addClass("fc-event-custom");
//            eventTime.html(eventTime.html().replace(/\d+:\d+\s-\s/, ""));

//        },
        viewDisplay: function () {
            if (window.appointmentModal)
                window.appointmentModal.close();
        },

        select: function (start, end, allDay) {

            if (window.appointmentModal)
                window.appointmentModal.close();

            window.appointmentModal = $.modal({
                url: window.createUrl,
                title: "Nova consulta",
                data: {
                    date: start.format("MM/dd/yyyy"),
                    start: start.format("HH:mm"),
                    end: end.format("HH:mm"),
                    doctorId: window.doctorId
                },
                width: 480,
                height: 200
            });
        },

        eventClick: function (event) {

            if (window.appointmentModal)
                window.appointmentModal.close();

            window.appointmentModal = $.modal({
                url: window.editUrl,
                title: "Editar consulta",
                data: {
                    id: event.id
                },
                width: 480,
                height: 200
            });

        }
    });

    fetchDatePickerEvents();
});