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
                window.datesWithAppointments = result
                $(".datepicker").datepicker("refresh");
            }
        });
    };

    $.datepicker.setDefaults($.datepicker.regional["pt-BR"]);
    $(".datepicker").datepicker({
        showOtherMonths: false,
        selectOtherMonths: false,
        onSelect: function (dateText, inst) {
            var match = dateText.match(/(\d{2})\/(\d{2})\/(\d{4})/);
            $('#calendar').fullCalendar('gotoDate', parseInt(match[3], 10), parseInt(match[2], 10) - 1, parseInt(match[1], 10));
        },
        beforeShowDay: function (date) {
            var month = date.getMonth() + 1;
            var day = date.getDate();
            var year = date.getFullYear();
            return [true, window.datesWithAppointments.indexOf($.datepicker.formatDate("'d'dd_mm_yy", date)) != -1 ? "dateWithAppointments" : ""];
        },
        onChangeMonthYear: function (year, month, inst) {
            fetchDatePickerEvents(year, month);
        }
    });


    $('#calendar').fullCalendar({

        header: { left: 'agendaWeek,agendaDay', center: '', right: 'today prev,next' },
        aspectRatio: 1,
        events: window.eventsUrl,
        weekends: window.weekends,
        selectable: true,
        defaultView: 'agendaWeek',
        // translation
        dayNames: ['Domingo', 'Segunda-Feira', 'Terça-Feira', 'Quarta-Feira', 'Quinta-Feira', 'Sexta-Feira', 'Sábado'],
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
            today: 'hoje',
            month: 'mês',
            week: 'semana',
            day: 'dia'
        },
        monthNames: ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'],
        monthNamesShort: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
        dayNamesShort: ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'],
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