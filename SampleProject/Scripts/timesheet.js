(function ($) {
    $.fn.getHours = function () {
        var totalMins = 0;
        this.each(function (i) {
            var timeParts = $(this).val().split(":");
            if (timeParts.length == 2) {
                totalMins = (parseInt(timeParts[0]) * 60) + parseInt(timeParts[1]);
            }
        });
        return isInteger(totalMins) ? Math.floor(totalMins / 60) : 0;
    }

    $.fn.getMins = function () {
        var totalMins = 0;
        this.each(function (i) {
            var timeParts = $(this).val().split(":");
            if (timeParts.length == 2) {
                totalMins = (parseInt(timeParts[0]) * 60) + parseInt(timeParts[1]);
            }
        });
        return isInteger(totalMins) ? Math.floor(totalMins % 60) : 0;
    }

    $.fn.getTime = function () {
        return this.val();
    }

    $.fn.setTime = function (time, triggerChange) {
        var timeParts = time.split(":");
        var hours = 0;
        var mins = 0;

        if (timeParts.length > 0) {
            hours = timeParts[0];
        }
        if (timeParts.length > 1) {
            mins = timeParts[1];
        }

        this.each(function (i) {
            if (!time) {
                $(this).val('');
            }
            else {
                $(this).val(hours.toString().padStart(2, "0") + ":" + mins.toString().padStart(2, "0"));

            }
            if (triggerChange)
                $(this).change();
        });
        return this; //for chaining
    }

})(jQuery);


Number.prototype.MinsToHHMM = function MinsToHHMM() {
    var hours = Math.floor(this / 60);
    var minutes = this % 60;
    return hours + ":" + minutes.toString().padStart(2, "0");
};

function isInteger(value) {
    if ((parseFloat(value) == parseInt(value)) && !isNaN(value)) {
        return true;
    } else {
        return false;
    }
}

function resetTimesheet() {
    $('#scheduler input.timePicker').val('');
    $('#scheduler input.hourPicker').val('');
    $('#scheduler input.notes').val('');
    recalculate();
}

function recalculate() {
    $("#scheduler input.dayTotal").each(function () {
        var row = $(this).closest("div.table-row");
        var totalDayTime = getDayTotal(row).MinsToHHMM();

        if ($(this).timeEntry('getTime') != totalDayTime)
            $(this).setTime(totalDayTime);
    });

    var totalWeekTime = getWeekTotal();
    $("#totalWeekHours").setTime(totalWeekTime.MinsToHHMM());
}

function getDayTotal(row) {
    var totalMins = 0;
    $("input.hourPicker", row).each(function () {
        if ($(this).val().length != 0) {
            var mins = (($(this).getHours() * 60) || 0) + $(this).getMins() || 0;
            totalMins += parseInt(mins);
        }
    });

    return totalMins;
}

function getWeekTotal() {
    var totalMins = 0;
    $("#scheduler input.dayTotal").each(function () {
        var mins = (($(this).getHours() * 60) || 0) + $(this).getMins() || 0;
        totalMins += parseInt(mins);
    });

    return totalMins;
}

function allocateHours() {
    $("#scheduler input.dayTotal").each(function () {
        allocateDayHours($(this));
    });
}

function allocateFromWeekTotal() {
    var weekTotal = $("#totalWeekHours").val().split(":");

    if (!isInteger(weekTotal[0]))
        weekTotal[0] = 0;

    if (!isInteger(weekTotal[1]))
        weekTotal[1] = 0;

    var totalMins = (parseInt(weekTotal[0]) * 60) + parseInt(weekTotal[1]);
    if (totalMins > 10080) {
        $("#totalWeekHours").setTime("168:00");
        totalMins = 10080;
    }
    var currentTotal = getWeekTotal();

    if (totalMins != currentTotal) {
        var hoursPerDay = Math.floor((totalMins / 60) / 7);
        var totalLeft = totalMins - (hoursPerDay * 7 * 60);
        var remainderHrs = Math.floor(totalLeft / 60);
        var remainderMins = totalLeft % 60;

        $("#scheduler input.dayTotal").each(function () {
            var addExtraHour = (remainderHrs > 0);
            $(this).setTime((hoursPerDay + (addExtraHour ? 1 : 0)) + ":" + (remainderMins), true);
            if (addExtraHour)
                remainderHrs--;
            remainderMins = 0;
        });

        allocateFromDayTotals();
    }
}

function allocateFromDayTotals() {
    $("#scheduler input.dayTotal").each(function () {
        allocateDayHours($(this));
    });
}

function allocateDayHours(dayTotalInputBox) {
    var dayTotal = dayTotalInputBox.getTime();
    var row = dayTotalInputBox.closest("div.table-row");
    var currentTotal = getDayTotal(row).MinsToHHMM();
    if (dayTotal != currentTotal) {
        $("input.timePicker", row).val('');
        $("input.hourPicker", row).val('');
        $("input.timePicker", row).first().setTime('9:00');
        $("input.hourPicker", row).first().setTime(dayTotal)/*.removeAttr("disabled")*/;
    }
}

function recalculateExpensesSummary() {
    var totalWeekExpenses = 0;
    $("#expenses td.expense input").each(function () {
        if ($(this).val().length > 0)
            totalWeekExpenses += parseFloat($(this).val());
    });
    $("#totalWeekExpenses").val(totalWeekExpenses).trigger('change');
}

function recalculateMilageSummary() {
    var totalWeekMiles = 0;
    $("#expenses td.mileage input").each(function () {
        if ($(this).val().length > 0)
            totalWeekMiles += parseFloat($(this).val());
    });
    $("#totalWeekMiles").val(totalWeekMiles);
}

function saveAndShowSummary()
{
    $('#showSummary').prop('checked', 'checked');
    $('#showExpenses').removeAttr('checked');
    $('#editForm').submit();
}

function saveTimesheet()
{
    $('#editForm').submit();
}

function saveAndShowExpenses() {
    $('#showExpenses').prop('checked', 'checked');
    $('#showSummary').removeAttr('checked');
    $('#editForm').submit();
}

function exportTimesheet(timesheetId) {

    $.form('/Timesheet/Export', { id: timesheetId }, 'POST').submit();
}

function GetAgreements(carerId) {
    var procemessage = "<option value='0'> Please wait...</option>";
    $("#agreementId").html(procemessage).show();
    var url = "/Timesheet/GetAgreementsForCarerJSON/";

    if (carerId == '')
        carerId = 0;


    $.ajax({
        url: url,
        data: { id: carerId },
        cache: false,
        success: function (data) {
            var markup = "<option value=''>Select Agreement</option>";
            for (var x = 0; x < data.length; x++) {
                markup += "<option value=" + data[x].Value + ">" + data[x].Text + "</option>";
            }
            $("#agreementId").html(markup).show();
        },
        error: function (reponse) {
            alert("error : " + reponse);
        }
    });
}

$(function () {
    onPageLoad();
});

function onPageLoad() {

    $('.datepicker').datepicker({
        daysOfWeekDisabled: [1, 2, 3, 4, 5, 6],
        format: 'dd/mm/yyyy',
        autoclose: true,
        weekStart: 1
    });

    $('#scheduler input.dayTotal:not([readonly])').timeEntry({
        show24Hours: true,
        showSeconds: false,
        spinnerImage: ''
    }).on('change', function () {
        allocateDayHours($(this));
    });

    $("#totalWeekHours").timeEntry({
        show24Hours: true,
        showSeconds: false,
        spinnerImage: '',
        unlimitedHours: true,
        minTime: '00:00',
        maxTime: '168:00',
        defaultTime: '00:00'
    }).on('keyup', function (e) {
        if (e.keyCode == 13) {
            allocateFromWeekTotal();
        }
        if (e.keyCode == 8) {
            $(this).setTime('');
            $(this).blur();
            $(this).focus();
        }
    }).focusout(function () {
        allocateFromWeekTotal();
    });

    $('#scheduler input.hourPicker:not([readonly])').timeEntry({
        show24Hours: true,
        showSeconds: false,
        spinnerImage: '',
        defaultTime: '00:00'
    }).on('change', function () {
        recalculate();
    }).on('keyup', function (e) {
        if (e.keyCode == 8) {
            $(this).setTime('');
            $(this).blur();
            $(this).focus();
        }
    })
    /*
            .each(function () {
            var val = $(this).val();
            if (val == "" || val.length == 0) {
                $(this).attr("disabled", "disabled");
            } else {
                $(this).removeAttr("disabled");
            }
        })*/;

    $('#scheduler input.timePicker:not([readonly])').timeEntry({
        show24Hours: true,
        showSeconds: false,
        spinnerImage: '',
        defaultTime: '09:00'
    }).on('keyup', function (e) {
        if (e.keyCode == 8) {
            $(this).setTime('');
            $(this).blur();
            $(this).focus();
        }
    })/*
        .on('change', function () {
        var hourPicker = $(this).parent().next().find('.hourPicker');
        var hasStartTime = $(this).val().length > 0;
        if (hasStartTime) {
            hourPicker.removeAttr("disabled");
        } else {
            hourPicker.setTime('');
            hourPicker.attr("disabled", "disabled");
        }
    })*/;

    $(".copytime").on('click', function () {
        var hours = [];
        var times = [];

        $(this).parent().parent().find('.hourPicker').each(function (i) {
            hours.push($(this).val());
        });

        $(this).parent().parent().next().find('.hourPicker').each(function (i) {
            $(this).val(hours[i])
        });

        $(this).parent().parent().find('.timePicker').each(function (i) {
            times.push($(this).val());
        });

        $(this).parent().parent().next().find('.timePicker').each(function (i) {
            $(this).val(times[i])
        });

        recalculate();
        return false;
    });

    recalculate();
    recalculateMilageSummary();
    recalculateExpensesSummary();

    $("#expenses td.expense input").on('change', function () {
        recalculateExpensesSummary();
    });

    $("#expenses td.mileage input").on('change', function () {
        recalculateMilageSummary();
    });
}