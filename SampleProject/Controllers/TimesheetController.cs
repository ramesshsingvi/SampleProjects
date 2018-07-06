/*
' Copyright (c) 2017  Blueclover Consulting Ltd
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.BookingService;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.ExpensesService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.RenderingService;
using TrustonTap.Common.Services.TimesheetService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Timesheets", "Index")]
    public class TimesheetController : BaseController
    {
        private ITimesheetService timesheetService;
        private IExpensesService expensesService;
        private IPaymentService paymentService;
        private ICarerService carerService;
        private IRenderingService renderingService;
        private ILoggingService loggingService;
        private IBookingService bookingService;

        public TimesheetController(        
            ITimesheetService timesheetService,
            IExpensesService expensesService,
            IPaymentService paymentService,
            ICarerService carerService,
            IRenderingService renderingService,
            ILoggingService loggingService,
            IBookingService bookingService)
        {
            this.renderingService = renderingService;
            this.timesheetService = timesheetService;
            this.expensesService = expensesService;
            this.paymentService = paymentService;
            this.carerService = carerService;
            this.loggingService = loggingService;
            this.bookingService = bookingService;
        }

        public ActionResult Index(bool includeInvoiced = false)
        {
            var timesheetData = timesheetService
                .GetTimesheets(null, null, null, includeInvoiced? (bool?)null : includeInvoiced, null, null, null, null)
                .ToList();

            return View(timesheetData);
        }

        #region Edit

        public ActionResult Edit(int id)
        {
            var timesheet = timesheetService.GetTimesheet(id);
            if (timesheet == null)
            {
                return HttpNotFound();
            }

            var canEdit = !(timesheet.CarerPaymentGenerated || timesheet.CustomerPaymentGenerated);
            var headerInfo = new Dictionary<string, string>
            {
                { "Booking", timesheet.Agreement.ID.ToString() },
                { "Week Ending", timesheet.WeekEnding.ToShortDateString() },
                { "Reference", timesheet.Reference },
                { "Customer", $"{timesheet.Agreement.CustomerDisplayName} [{timesheet.Agreement.CustomerID}]" },
                { "Care Recipient", $"{timesheet.Agreement.CareRecipientDisplayName}" + (timesheet.Agreement.CareRecipientID.HasValue ? $" [{timesheet.Agreement.CareRecipientID}]" : "") },
                { "Payer", timesheet.Agreement.PayerDisplayName  + (timesheet.Agreement.PayerId.HasValue ? $" [{timesheet.Agreement.PayerId}]" : "") },
                { "Submitted By", timesheet.SubmittedBy }
            };

            if (timesheet.SubmittedDate.HasValue)
            {
                headerInfo.Add("Submitted Date", timesheet.SubmittedDate.Value.ToString("g"));
            }

            ViewBag.CanEdit = canEdit;
            ViewBag.HeaderInfo = headerInfo;

            var model = TimesheetWeekViewModel.ToViewModel(timesheet);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(TimesheetWeekViewModel timesheet)
        {
            var success = true;

            try
            {
                var canEdit = !(timesheet.CarerPaymentGenerated || timesheet.CustomerPaymentGenerated);
                if (canEdit)
                {
                    SaveTimesheet(timesheet);
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            TempData["ShowExpenses"] = timesheet.ShowExpensesModal;
            TempData["ShowSummary"] = timesheet.ShowSummaryModal;

            if (!timesheet.ShowExpensesModal && !timesheet.ShowSummaryModal)
            {
                SetFeedbackMessage(success,
                    "Timesheet has been saved.",
                    "There has been an error whilst saving the timesheet.");
            }

            return RedirectToSamePage();
        }

        #endregion

        #region Export

        public FileResult Export(int id)
        {
            var week = timesheetService.GetTimesheet(id);
            var dataTable = week.ToDataTable();

            var template = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"bin\Templates\Timesheet.cshtml");
            var viewBag = new Dictionary<string, object>() {
                { "MileageAllowance", ApplicationSettings.MileageAllowance }
            };
            var html = renderingService.RenderHtml(week, template, viewBag);
            var documentContent = renderingService.RenderPdf(html);

            var mimeType = Utilities.GetMimeType(".pdf");
            var file = new FileContentResult(documentContent, mimeType)
            {
                FileDownloadName = "Timesheet.pdf"
            };

            return file;
        }

        #endregion

        #region Submit

        [HttpPost]
        public ActionResult Submit(int ID)
        {
            var success = true;

            try
            {
                var timesheet = timesheetService.GetTimesheet(ID);
                SubmitTimesheet(timesheet);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Timesheet has been submitted",
                "There has been an error whilst submitting the timesheet");

            return RedirectToAction("Index");
        }

        #endregion

        #region Expenses

        [HttpPost]
        public ActionResult Expenses(List<ExpenseViewModel> expenses)
        {
            var success = true;

            try
            {
                var daysOfTheWeek = expenses.Select(x => x.ExpenseDate).Distinct().OrderBy(x => x.Date);

                foreach (var day in daysOfTheWeek)
                {
                    var expensesForDay = expenses.Where(x => x.ExpenseDate == day);
                    var notes = expensesForDay.FirstOrDefault(x => !String.IsNullOrEmpty(x.Notes))?.Notes;
                    foreach (var expense in expensesForDay)
                    {
                        switch (expense.Type)
                        {
                            case ExpenseType.Mileage:
                                expense.ExtraInfo = expense.Miles.ToString();
                                expense.AmountClaimed = expense.Miles * ApplicationSettings.MileageAllowance;
                                break;

                            case ExpenseType.Other:
                                expense.Notes = notes;
                                break;
                        }
                        if (expense.ID > 0)
                        {
                            var oldExpense = expensesService.GetExpense(expense.ID);

                            if (expense.AmountClaimed > 0)
                            {
                                oldExpense.AmountClaimed = expense.AmountClaimed;
                                oldExpense.ExtraInfo = expense.ExtraInfo;
                                oldExpense.Notes = notes;
                                expensesService.Save(oldExpense);
                            }
                            else
                            {
                                if (oldExpense != null)
                                {
                                    expensesService.DeleteExpense(expense.ID);
                                }
                            }
                        }
                        else
                        {
                            if (expense.AmountClaimed > 0)
                            {
                                var newExpense = new Expense()
                                {
                                    AgreementID = expense.AgreementID,
                                    CarerID = expense.CarerID,
                                    ExtraInfo = expense.ExtraInfo,
                                    AmountClaimed = expense.AmountClaimed,
                                    ExpenseDate = expense.ExpenseDate,
                                    Notes = notes,
                                    ExpenseTypeID = expense.ExpenseTypeID
                                };

                                expensesService.Save(newExpense);

                                var timesheet = timesheetService.GetTimesheet(expense.TimesheetID);
                                if (timesheet != null)
                                {
                                    if (timesheet.Status == TimesheetStatus.Submitted || timesheet.Status == TimesheetStatus.Approved)
                                    {
                                        expensesService.Submit(newExpense);
                                    }
                                    if (timesheet.Status == TimesheetStatus.Approved)
                                    {
                                        expensesService.Approve(newExpense);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Expenses have been updated.",
                "There has been an error whilst saving the expenses.");
            return RedirectToSamePage();
        }

        #endregion

        #region New

        [HttpPost]
        public ActionResult New(long agreementId, DateTime weekEnding)
        {
            var booking = bookingService.GetBookingByReference(agreementId.ToString());

            if (booking != null && booking.CarerID.HasValue)
            {
                var timesheet = timesheetService.GetTimesheets(booking.CarerID.Value, null, null, null, null, null, null, null)
                    .Where(x => x.WeekEnding == weekEnding && x.AgreementID == agreementId)
                    .FirstOrDefault();

                if (timesheet == null)
                {
                    timesheet = new TimesheetWeek(weekEnding)
                    {
                        AgreementID = agreementId,
                        CarerID = booking.CarerID.Value
                    };

                    timesheetService.Save(timesheet);
                }

                return RedirectToAction("Edit", new { Id = timesheet.ID });
            }

            return RedirectToSamePage();
        }

        #endregion

        #region Unsubmit

        public ActionResult Unsubmit(int id)
        {
            var success = true;
            try
            {
                var timesheet = timesheetService.GetTimesheet(id);

                if (timesheet != null)
                {
                    timesheetService.Reject(timesheet);
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }
            SetFeedbackMessage(success,
                "Timesheet has been unsubmitted",
                "There has been an error whilst unsubmitting the timesheet");

            return RedirectToAction("Edit", new { Id = id });
        }

        #endregion

        #region Delete

        public ActionResult Delete(int id)
        {
            timesheetService.Delete(id);

            return RedirectToAction("Index");
        }

        #endregion

        #region View

        public ActionResult ViewByReference(string reference)
        {
            var timesheet = timesheetService.GetTimesheetByReference(reference);
            if (timesheet == null)
            {
                return HttpNotFound();
            }

            return View(timesheet.ID);
        }

        public ActionResult View(int Id)
        {
            var timesheet = timesheetService.GetTimesheet(Id);
            if(timesheet == null)
            {
                return HttpNotFound();
            }

            var headerInfo = new Dictionary<string, string>
            {
                { "Booking", timesheet.Agreement.ID.ToString() },
                { "Week Ending", timesheet.WeekEnding.ToShortDateString() },
                { "Reference", timesheet.Reference },
                { "Customer", $"<a href='/Customer/Details/{timesheet.Agreement.CarerID}'>{timesheet.Agreement.CustomerDisplayName} [{timesheet.Agreement.CustomerID}]</a>"},
                { "Care Recipient", $"{timesheet.Agreement.CareRecipientDisplayName}" + (timesheet.Agreement.CareRecipientID.HasValue ? $" [{timesheet.Agreement.CareRecipientID}]" : "") },
                { "Payer", $"<a href='/Customer/Details/{timesheet.Agreement.PayerId}'>{timesheet.Agreement.PayerDisplayName} [{timesheet.Agreement.PayerId}]</a>"},
                { "Submitted By", timesheet.SubmittedBy },
                { "Agreed Hours", (timesheet.Agreement.AgreedHours* 60).ConvertMinutesToHHMMString() },
                { "Hourly Rate", (timesheet.Agreement.HourlyRate).ToMoney() }
            };

            if (timesheet.SubmittedDate.HasValue)
            {
                headerInfo.Add("Submitted Date", timesheet.SubmittedDate.Value.ToString("g"));
            }

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.MileageAllowance = ApplicationSettings.MileageAllowance;

            return View("View", timesheet);
        }

        #endregion

        public ActionResult HoldFromCustomerInvoice(int id, bool hold = true)
        {
            {
                var success = true;
                try
                {
                    var timesheet = timesheetService.GetTimesheet(id);

                    if (timesheet != null)
                    {
                        timesheetService.HoldFromCustomerInvoice(timesheet.Reference, hold);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    loggingService.LogException(ex);
                }
                SetFeedbackMessage(success,
                    "Timesheet is " + (hold?"":"no longer ") + "being held back from customer invoice",
                    "There has been an error whilst " + (hold ? "" : "ending ") + "holding back the timesheet");

                return RedirectToAction("Index");
            }
        }

        public ActionResult HoldFromCarerStatement(int id, bool hold = true)
        {
            {
                var success = true;
                try
                {
                    var timesheet = timesheetService.GetTimesheet(id);

                    if (timesheet != null)
                    {
                        timesheetService.HoldFromCarerStatement(timesheet.Reference, hold);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    loggingService.LogException(ex);
                }
                SetFeedbackMessage(success,
                    "Timesheet is " + (hold ? "" : "no longer ") + "being held back from carer statement",
                    "There has been an error whilst " + (hold ? "" : "ending ") + "holding back the timesheet");

                return RedirectToSamePage();
            }
        }

        #region Ajax

        public JsonResult GetAgreementsForCarerJSON(int id)
        {
            SelectList bookingSelectList = null;
            try
            {
                IEnumerable<Booking> bookings;
                if (id==0)
                {
                    bookings = bookingService.GetBookings()
                        .Where(x => x.BookingStatus == BookingStatus.Live);
                }
                else
                {
                    bookings = bookingService.GetBookingsForCarer(id)
                        .Where(x => x.BookingStatus == BookingStatus.Live);
                }

                bookingSelectList = new SelectList(bookings, "Reference", "Title", 0);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);
            }

            return Json(bookingSelectList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private Methods

        private void SaveTimesheet(TimesheetWeekViewModel timesheet, bool submit = false)
        {
            var oldTimesheet = timesheetService.GetTimesheet(timesheet.ID);
            oldTimesheet.Notes = timesheet.Notes;

            for (int i = 0; i < oldTimesheet.Days.Count; i++)
            {
                oldTimesheet.Days[i].Notes = timesheet.Days[i].Notes;
                for (int j = 0; j < oldTimesheet.Days[i].Periods.Count; j++)
                {
                    var startTime = Utilities.MergeDateAndTime(oldTimesheet.Days[i].Date, timesheet.Days[i].Periods[j].StartTime);
                    oldTimesheet.Days[i].Periods[j].StartTime = startTime;

                    var minsWorked = timesheet.Days[i].Periods[j].NumberOfHours.ConvertHHMMStringToMinutes();                 
                    oldTimesheet.Days[i].Periods[j].EndTime = startTime.AddMinutes(minsWorked);
                }
            }
            timesheetService.Save(oldTimesheet);
            if (submit)
            {
                SubmitTimesheet(oldTimesheet);
            }
        }

        private void SubmitTimesheet(TimesheetWeek timesheet)
        {
            timesheet.SubmittedByAdmin = true;
            timesheetService.Submit(timesheet);
            timesheetService.Approve(timesheet);

            timesheet.Expenses.ForEach(expense =>
            {
                expensesService.Submit(expense);
                expensesService.Approve(expense);
            });
        }

        #endregion

        #region Modal Popup Windows

        public ActionResult NewTimesheetModal()
        {
            var carers = carerService.GetCarers()
                .Where(x=>!String.IsNullOrWhiteSpace(x.FormattedName))
                .ToList();

            return PartialView("_NewTimesheetModal", carers);
        }

        public ActionResult SummaryModal(int Id)
        {
            var timesheet = timesheetService.GetTimesheet(Id);
            ViewBag.MileageAllowance = ApplicationSettings.MileageAllowance;

            var validationMessage = "";
            ViewBag.IsValid = timesheetService.ValidateTimesheet(timesheet, out validationMessage);
            ViewBag.ValidationMessage = validationMessage;

            return PartialView("_SummaryModal", timesheet);
        }

        public ActionResult ViewModal(int Id)
        {
            var timesheet = timesheetService.GetTimesheet(Id);
            var headerInfo = new Dictionary<string, string>
            {
                { "Booking", timesheet.Agreement.ID.ToString() },
                { "Week Ending", timesheet.WeekEnding.ToShortDateString() },
                { "Reference", timesheet.Reference },
                { "Customer", $"{timesheet.Agreement.CustomerDisplayName} [{timesheet.Agreement.CustomerID}]" },
                { "Care Recipient", $"{timesheet.Agreement.CareRecipientDisplayName}" + (timesheet.Agreement.CareRecipientID.HasValue ? $" [{timesheet.Agreement.CareRecipientID}]" : "") },
                { "Payer", timesheet.Agreement.PayerDisplayName  + (timesheet.Agreement.PayerId.HasValue ? $" [{timesheet.Agreement.PayerId}]" : "") },
                { "Submitted By", timesheet.SubmittedBy },
                { "Agreed Hours", (timesheet.Agreement.AgreedHours* 60).ConvertMinutesToHHMMString() },
                { "Hourly Rate", (timesheet.Agreement.HourlyRate).ToMoney() }
            };

            if (timesheet.SubmittedDate.HasValue)
            {
                headerInfo.Add("Submitted Date", timesheet.SubmittedDate.Value.ToString("g"));
            }

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.MileageAllowance = ApplicationSettings.MileageAllowance;

            return PartialView("_ViewModal", timesheet);
        }

        public ActionResult ExpensesModal(int id)
        {
            var timesheet = timesheetService.GetTimesheet(id);

            var mon = timesheet.WeekEnding.GetWeekStartDate();
            var tue = mon.AddDays(1);
            var wed = mon.AddDays(2);
            var thu = mon.AddDays(3);
            var fri = mon.AddDays(4);
            var sat = mon.AddDays(5);
            var sun = mon.AddDays(6);

            var expenses = expensesService.GetExpenses(timesheet.CarerID, null, null, null)
                .Where(x => x.AgreementID == timesheet.AgreementID && x.ExpenseDate >= mon && x.ExpenseDate <= sun)
                .ToList();

            foreach (var date in new DateTime[] { mon, tue, wed, thu, fri, sat, sun })
            {
                var dayExpenses = expenses.Where(x => x.ExpenseDate == date);
                {
                    if (!dayExpenses.Any(x => x.Type == ExpenseType.Mileage))
                    {
                        expenses.Add(new Expense() { CarerID = timesheet.CarerID, AgreementID = timesheet.AgreementID, Type = ExpenseType.Mileage, ExpenseDate = date });
                    }
                    if (!dayExpenses.Any(x => x.Type == ExpenseType.Other))
                    {
                        expenses.Add(new Expense() { CarerID = timesheet.CarerID, AgreementID = timesheet.AgreementID, Type = ExpenseType.Other, ExpenseDate = date });
                    }
                }
            }

            var canEdit = !(timesheet.CarerPaymentGenerated || timesheet.CustomerPaymentGenerated);

            ViewBag.CanEdit = canEdit;
            ViewBag.WeekEnding = timesheet.WeekEnding;
            ViewBag.Carer = timesheet.Carer;
            ViewBag.CareRecipient = timesheet.Agreement.CareRecipientDisplayName;

            var model = expenses.Select(x => ExpenseViewModel.ToViewModel(x)).ToList();
            model.ForEach(x =>
            {
                if (x.Type == ExpenseType.Mileage && !string.IsNullOrWhiteSpace(x.ExtraInfo))
                {
                    x.Miles = decimal.Parse(x.ExtraInfo);
                }
                x.TimesheetID = id;
            });

            return PartialView("_ExpensesModal", model);
        }


        #endregion

    }
}