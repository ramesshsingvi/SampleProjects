
(function ($) {
    $.fn.currencyFormat = function () {
        this.each(function (i) {
            $(this).change(function (e) {
                if (isNaN(parseFloat(this.value))) return;
                this.value = parseFloat(this.value).toFixed(2);
            });

            if (isNaN(parseFloat(this.value))) return;
            this.value = parseFloat(this.value).toFixed(2);
        });
        return this; //for chaining
    }

    $.fn.formNavigation = function () {
        $(this).each(function () {
            $(this).find('input').on('keyup', function (e) {
                switch (e.which) {
                    case 39:
                        $(this).closest('.table-cell').next().find('input').focus(); break;
                    case 37:
                        $(this).closest('.table-cell').prev().find('input').focus(); break;
                    case 40:
                        $(this).closest('.table-row').next().children().eq($(this).closest('.table-cell').index()).find('input').focus(); break;
                    case 38:
                        $(this).closest('.table-row').prev().children().eq($(this).closest('.table-cell').index()).find('input').focus(); break;
                }
            });
        });
    };

    $.extend({
        form: function (url, data, method) {
            if (method == null) method = 'POST';
            if (data == null) data = {};

            var form = $('<form>').attr({
                method: method,
                action: url
            }).css({
                display: 'none'
            });

            var addData = function (name, data) {
                if ($.isArray(data)) {
                    for (var i = 0; i < data.length; i++) {
                        var value = data[i];
                        addData(name + '[]', value);
                    }
                } else if (typeof data === 'object') {
                    for (var key in data) {
                        if (data.hasOwnProperty(key)) {
                            addData(name + '[' + key + ']', data[key]);
                        }
                    }
                } else if (data != null) {
                    form.append($('<input>').attr({
                        type: 'hidden',
                        name: String(name),
                        value: String(data)
                    }));
                }
            };

            for (var key in data) {
                if (data.hasOwnProperty(key)) {
                    addData(key, data[key]);
                }
            }

            return form.appendTo('body');
        }
    });

    applyStyles();
})(jQuery);

function applyStyles() {
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        weekStart: 1
    });

    $('.weekendingpicker').datepicker({
        daysOfWeekDisabled: [1, 2, 3, 4, 5, 6],
        format: 'dd/mm/yyyy',
        autoclose: true,
        weekStart: 1
    });

    $('.currency').currencyFormat();

    if (typeof (CKEDITOR) !== "undefined") {
        $('.richtext').ckeditor();
    }

    $('[data-toggle=confirmation]').confirmation({
        rootSelector: '[data-toggle=confirmation]',
        // other options
    });

    $(function () {
        // Enables popover
        $("[data-toggle=popover]").css('cursor', 'pointer').popover();

        $("[data-toggle=popoverfull]").css('cursor', 'pointer').popover({ container: 'body' })
            .on("show.bs.popover", function () { $(this).data("bs.popover").tip().css("max-width", "600px").css("max-height", "400px").css("overflow-y", "auto"); });
    });

    $('.draggable-modal').modal({
        backdrop: false,
        keyboard: false,
        show: true
    });
    // Jquery draggable
    $('.modal-dialog').draggable({
        handle: ".modal-header"
    });

    //if (!($('.modal.in').length)) {
        $('.modal-dialog').css({
            top: 0,
            left: 0
        });
    //}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Booking Functions
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////
// Search Bookings
/////////////////////////////////////////
function showSearchBookingsModal() {
    $.ajax({
        url: "/Booking/SearchModal/",
        cache: false,
        type: "Post",
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#searchModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Change Booking Status
/////////////////////////////////////////
function showChangeStatusModal(bookingID, newStatus) {
    $.ajax({
        url: "/Booking/ChangeStatusModal/",
        cache: false,
        type: "Post",
        data: {
            id: bookingID,
            newStatus: newStatus
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#statusModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Show Standard Wording Modal
/////////////////////////////////////////
function showStandardWordingModal(category, textField) {
    $.ajax({
        type: "GET",
        url: "/Booking/GetStandardWording",
        data: { category: category },
        success: function (response) {
            $('#modalPlaceholder').html(response);
            $('#standardWordingModal').modal('show');
            applyStyles();

            $(".wording").click(function () {
                var text = textField.val();
                text += $(this).html();
                textField.val(text);
                $('#standardWordingModal').modal('hide');
            });
        },
    })
}


/////////////////////////////////////////
// Show Booking Panel
/////////////////////////////////////////
function showBookingPanel(panel, show) {
    if (show) {
        panel.parents('.searchpanel').find('.panel-body').slideDown();
        panel.removeClass('panel-collapsed');
        panel.find('i').removeClass('glyphicon-chevron-down').addClass('glyphicon-chevron-up');
    } else {
        panel.parents('.searchpanel').find('.panel-body').slideUp();
        panel.addClass('panel-collapsed');
        panel.find('i').removeClass('glyphicon-chevron-up').addClass('glyphicon-chevron-down');
    }
}

/////////////////////////////////////////
// Booking Summary
/////////////////////////////////////////
function showBookingViewModal(bookingID) {
    $.ajax({
        url: "/Booking/ViewModal/",
        cache: false,
        type: "Post",
        data: {
            id: bookingID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#viewModal').modal('show');

            applyStyles();
        }
    });
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////
// Take Payments
/////////////////////////////////////////
function showTakePaymentModal(customerID) {
    $.ajax({
        url: "/CustomerPayment/TakePaymentModal/",
        cache: false,
        type: "Post",
        data: {
            id: customerID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#paymentModal').modal('show');

            applyStyles();
        }
    });
}

function showEditPaymentModal(transactionID) {
    $.ajax({
        url: "/CustomerPayment/EditModal/",
        cache: false,
        type: "Post",
        data: {
            id: transactionID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#editModal').modal('show');

            applyStyles();
        }
    });
}

function setPaymentAmount(amount) {
    $('#amount').val(amount);
    $('#amount').change();
}

function updateAllocationTotal() {
    var totalAllocated = 0;
    $(".allocationAmount").each(function () {
        var totalToAllocate = parseFloat($(this).val());

        var max = parseFloat($(this).closest('.row').find('.amountOutstanding').val());
        var allocationAmount = Math.min(max, totalToAllocate);
        if (totalToAllocate != allocationAmount)
        {
            $(this).val(allocationAmount);
        }
        totalAllocated += parseFloat(allocationAmount);
    });
    $("#totalAllocated").val(totalAllocated.toFixed(2));
}

function allocatePayment(amount) {
    var totalToAllocate = amount;
    $(".allocationAmount").each(function () {
        var max = parseFloat($(this).closest('.row').find('.amountOutstanding').val());
        var allocationAmount = Math.min(max, totalToAllocate);
        $(this).val(allocationAmount);
        $(this).change();
        totalToAllocate -= allocationAmount;
        if (totalToAllocate <= 0) {
            totalToAllocate = 0;
        }
    });
}

function toggleAllocationPanel(checkbox) {
    if (checkbox.checked) {
        var totalToAllocate = $('#amount').val();
        allocatePayment(totalToAllocate);
        $('#allocationPanel').show();
    } else {
        allocatePayment(0);
        $('#allocationPanel').hide();
    }
}

/////////////////////////////////////////
// Make Payments
/////////////////////////////////////////
function showMakePaymentModal() {

    var total = $("#totalSelected").html();
    var jdata = [];
    $('input:checked.toggleable').each(function () {
        jdata.push($(this).val());
    })

    $.ajax({
        url: "/CarerPayment/MakePaymentModal/",
        cache: false,
        type: "Post",
        data: {
            ids: jdata,
            totalAmount: total
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#paymentModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Take Payments - GoCardless
/////////////////////////////////////////
function showTakePaymentGoCardlessModal() {

    var total = $("#totalSelected").html();
    var jdata = [];
    $('input:checked.toggleable').each(function () {
        jdata.push($(this).val());
    })

    $.ajax({
        url: "/CustomerPayment/TakePaymentGoCardlessModal/",
        cache: false,
        type: "Post",
        data: {
            ids: jdata,
            totalAmount: total
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#paymentModal').modal('show');

            applyStyles();
        }
    });
}

function exportPaymentsGoCardless() {
    var ids = [];
    $('input:checked.toggleable').each(function () {
        ids.push($(this).val());
    })

    $.form('/CustomerPayment/ExportGoCardless', { ids: ids }, 'POST').submit();
}

/////////////////////////////////////////
// Allocate Payments
/////////////////////////////////////////
function showAllocationModal(transactionID) {
    $.ajax({
        url: "/CustomerPayment/AllocationModal/",
        cache: false,
        type: "Post",
        data: {
            id: transactionID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#allocationModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Transfer Payments
/////////////////////////////////////////
function showTransferModal(transactionID) {
    $.ajax({
        url: "/CustomerPayment/TransferUnallocatedModal/",
        cache: false,
        type: "Post",
        data: {
            id: transactionID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#transferModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Refund Payments
/////////////////////////////////////////
function showRefundModal(transactionID) {
    $.ajax({
        url: "/CustomerPayment/RefundModal/",
        cache: false,
        type: "Post",
        data: {
            id: transactionID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#refundModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Expenses
/////////////////////////////////////////
function showExpensesModal(timesheetID) {
    $.ajax({
        url: "/Timesheet/ExpensesModal/",
        cache: false,
        type: "Post",
        data: {
            id: timesheetID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#expensesModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// New User
/////////////////////////////////////////

function saveAndShowNewCustomerModal() {
    $('#showNewCustomer').prop('checked', 'checked');
    $('#showNewPayer').removeAttr('checked');
    $('#showNewCareRecipient').removeAttr('checked');
    $('#showNewNote').removeAttr('checked');
    $('#showStatusChangeModal').removeAttr('checked');
    $('#showBookingPreview').removeAttr('checked');
    $('#btnSave').click();
}

function saveAndShowNewPayerModal() {
    $('#showNewPayer').prop('checked', 'checked');
    $('#showBookingPreview').removeAttr('checked');
    $('#showNewCustomer').removeAttr('checked');
    $('#showNewCareRecipient').removeAttr('checked');
    $('#showNewNote').removeAttr('checked');
    $('#showStatusChangeModal').removeAttr('checked');
    $('#btnSave').click();
}

function saveAndShowNewCareRecipientModal() {
    $('#showNewCareRecipient').prop('checked', 'checked');
    $('#showBookingPreview').removeAttr('checked');
    $('#showNewPayer').removeAttr('checked');
    $('#showNewCustomer').removeAttr('checked');
    $('#showNewNote').removeAttr('checked');
    $('#showStatusChangeModal').removeAttr('checked');
    $('#btnSave').click();
}

function saveAndShowNewNoteModal() {
    $('#showNewNote').prop('checked', 'checked');
    $('#showBookingPreview').removeAttr('checked');
    $('#showNewCareRecipient').removeAttr('checked');
    $('#showNewPayer').removeAttr('checked');
    $('#showNewCustomer').removeAttr('checked');
    $('#showStatusChangeModal').removeAttr('checked');
    $('#btnSave').click();
}

function saveAndShowBookingStatusModal(bookingID, newStatus) {
    $('#NewBookingStatus').val(newStatus);
    $('#showBookingPreview').removeAttr('checked');
    $('#showStatusChangeModal').prop('checked', 'checked');
    $('#showNewCareRecipient').removeAttr('checked');
    $('#showNewPayer').removeAttr('checked');
    $('#showNewCustomer').removeAttr('checked');
    $('#showNewNote').removeAttr('checked');
    $('#btnSave').click();
}

function saveAndShowBookingPreviewEmailModal() {
    $('#showBookingPreview').prop('checked', 'checked');
    $('#showNewPayer').removeAttr('checked');
    $('#showNewCareRecipient').removeAttr('checked');
    $('#showNewNote').removeAttr('checked');
    $('#showStatusChangeModal').removeAttr('checked');
    $('#showNewCustomer').removeAttr('checked');
    $('#btnSave').click();
}

function showNewUserModal(bookingId, userType, title) {
    $.ajax({
        url: "/User/NewUserModal/",
        cache: false,
        type: "Post",
        data: {
            bookingId: bookingId,
            title: title,
            userType: userType
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#newUserModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Search Carer Statement
/////////////////////////////////////////
function showSearchCarerStatementModal() {
    $.ajax({
        url: "/CarerStatement/SearchModal/",
        cache: false,
        type: "Post",
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#searchModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Add Booking Note
/////////////////////////////////////////
function showBookingNoteModal(bookingID) {
    $.ajax({
        url: "/Booking/AddBookingNoteModal/",
        cache: false,
        type: "Post",
        data: {
            id: bookingID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#noteModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Booking Preview Email
/////////////////////////////////////////
function showBookingPreviewEmailModal(bookingID) {
    $.ajax({
        url: "/Booking/ShowBookingPreviewEmailModal/",
        cache: false,
        type: "Post",
        data: {
            id: bookingID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#messageModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Search Customer Invoice
/////////////////////////////////////////
function showSearchCustomerInvoiceModal() {
    $.ajax({
        url: "/CustomerInvoice/SearchModal/",
        cache: false,
        type: "Post",
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#searchModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Timesheets
/////////////////////////////////////////
function showNewTimesheetModal() {
    $.ajax({
        url: "/Timesheet/NewTimesheetModal/",
        cache: false,
        type: "Post",
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#timesheetModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Timesheet Summary
/////////////////////////////////////////
function showSummaryModal(timesheetID) {
    $.ajax({
        url: "/Timesheet/SummaryModal/",
        cache: false,
        type: "Post",
        data: {
            id: timesheetID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#summaryModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Timesheet Summary
/////////////////////////////////////////
function showTimesheetViewModal(timesheetID) {
    $.ajax({
        url: "/Timesheet/ViewModal/",
        cache: false,
        type: "Post",
        data: {
            id: timesheetID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#viewModal').modal('show');

            applyStyles();
        }
    });
}


/////////////////////////////////////////
// Post Invoices
/////////////////////////////////////////
function showPostInvoicesModal(batchID) {
    $.ajax({
        url: "/CustomerInvoice/SendCustomerInvoicesModal/",
        cache: false,
        type: "Post",
        data: {
            id: batchID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#messageModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Edit Holiday
/////////////////////////////////////////
function showEditHolidayModal(holidayID) {
    $.ajax({
        url: "/Holiday/EditModal/",
        cache: false,
        type: "Post",
        data: {
            id: holidayID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#holidayModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Add Holiday
/////////////////////////////////////////
function showAddHolidayModal() {
    $.ajax({
        url: "/Holiday/AddModal/",
        cache: false,
        type: "Post",
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#holidayModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Post Statements
/////////////////////////////////////////
function showPostCarerStatementsModal(batchID) {
    $.ajax({
        url: "/CarerStatement/SendCarerStatementsModal/",
        cache: false,
        type: "Post",
        data: {
            id: batchID,
        },
        success: function (html) {
            $("#modalPlaceholder").html(html);
            $('#messageModal').modal('show');

            applyStyles();
        }
    });
}

/////////////////////////////////////////
// Availability
/////////////////////////////////////////
function saveAvailability() {
    $('#editForm').submit();
}

/////////////////////////////////////////
// Misc UI
/////////////////////////////////////////
var toggled = false;
function toggleAllCheckboxes() {
    toggled = !toggled;
    var checkBoxes = $("input:checkbox.toggleable");
    checkBoxes.prop("checked", toggled).change();
}
/////////////////////////////////////////
// Send SMS
/////////////////////////////////////////
function openSendSmsWindow(carerId, phone, src)
{
    var url ="/Sms/Index?phone=" + phone + "&carerId=" + carerId + "&isPopup=true&src=" + src
    var windowoption='resizable=yes,height=600,width=600,location=0,menubar=0,scrollbars=1';
    var wnd = window.open(url, "SendSMS", windowoption);
    wnd.focus();
}

/////////////////////////////////////////
// Send SMS
/////////////////////////////////////////
function OnStandardResponseSelect(selValue)
{
    //var val = $("#Message").val();
    //if (selValue != "")
    //{
    //    val += selValue;
    //}
    $("#Message").val(selValue);
    $("#Message").focus();
    $("#Message").blur();
}
function CloseWindow() {
    window.close();
    return false;
}