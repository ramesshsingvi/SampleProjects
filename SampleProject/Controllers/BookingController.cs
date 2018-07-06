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
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.BookingService;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.MessagingService;
using TrustonTap.Common.Services.RenderingService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Services.MessagingService.EmailAccounts;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Bookings", "Index")]
    public class BookingController : BaseController
    {
        private IBookingService bookingService;
        private ILoggingService loggingService;
        private IRenderingService renderingService;
        private IMessagingService messagingService;
        private IUserService userService;
        private ICarerService carerService;

        public BookingController(
            ILoggingService loggingService,
            IBookingService bookingService,
            IMessagingService messagingService,
            IRenderingService renderingService,
            IUserService userService,
            ICarerService carerService)
        {
            this.bookingService = bookingService;
            this.loggingService = loggingService;
            this.messagingService = messagingService;
            this.renderingService = renderingService;
            this.userService = userService;
            this.carerService = carerService;
        }

        public ActionResult Index()
        {
            var bookings = bookingService.GetBookings()
                .OrderByDescending(x=>x.UpdateDate)
                .ThenByDescending(x => x.CreatedDate)
                .ToList();

            return View(bookings);
        }

        public ActionResult New()
        {
            var model = new BookingViewModel();
            return View("Edit", model);
        }

        public ActionResult EditByReference(string reference)
        {
            var booking = bookingService.GetBookingByReference(reference);
            return RedirectToAction("Edit", new { id = booking.ID });
        }

        public ActionResult Edit(int id, string mode = "")
        {
            var booking = bookingService.GetBooking(id);
            if (booking == null)
                return new HttpNotFoundResult("Booking not found");

            var model = new BookingViewModel(booking);

            #region Header Info

            var headerInfo = new Dictionary<string, string>();

            if (!booking.IsNew())
            {
                headerInfo.Add("Booking Type", booking.CareType.GetDescription());
                if (booking.MaxWeeklyHours.HasValue && booking.HourlyRate.HasValue)
                {
                    headerInfo.Add("Max Hours", booking.MaxWeeklyHours.ToString());
                    headerInfo.Add("Rate", booking.HourlyRate.Value.ToMoney());
                }
            }

            if (booking.CustomerID.HasValue && booking.Customer != null)
            {
                headerInfo.Add("Customer", String.Format("<a target='_blank' href='{0}'>{1}</a>", Url.Action("Details", "Customer", new { Id = booking.Customer.ID }), booking.Customer.DisplayName));
            }
            if (booking.CareRecipientID.HasValue && booking.CareRecipient != null)
            {
                headerInfo.Add("Care Recipient", booking.CareRecipient.FormattedName);
            }
            if (booking.CarerID.HasValue && booking.Carer != null)
            {
                headerInfo.Add("Carer", String.Format("<a target='_blank' href='{0}'>{1}</a>", Url.Action("Details", "Carer", new { Id = booking.Carer.ID }), booking.Carer.DisplayName));
            }

            headerInfo.Add("Created Date", booking.CreatedDate.ToString("dd/MM/yyyy HH:mm"));
            headerInfo.Add("Created By", booking.GetCreatedByUser()?.FormattedName);

            if(booking.UpdateDate.HasValue)
            {
                headerInfo.Add("Last Updated", booking.UpdateDate.Value.ToString("dd/MM/yyyy HH:mm"));
                headerInfo.Add("Updated By", booking.GetUpdatedByUser()?.FormattedName);
                if (booking.Timesheets.Any())
                {
                    headerInfo.Add("Number Of Timesheets", String.Format("<a target='_blank' href='{0}?includeInvoiced=true&grid-filter=Reference__3__{2}-'>{1}</a>", Url.Action("Index", "Timesheet"), booking.Timesheets.Count.ToString(), booking.Reference));
                }
                if (booking.Invoices.Any())
                {
                    headerInfo.Add("Value Of Billings", String.Format("<a target='_blank' href='{0}?StatementReference={2}-'>{1}</a>", Url.Action("Search", "CustomerInvoice"), booking.Invoices.Sum(x => x.Total).ToMoney(), booking.Reference));
                }
            }

            #endregion

            if (TempData["NewBookingStatus"] != null)
            {
                model.NewBookingStatus = ((BookingStatus)TempData["NewBookingStatus"]);
            }

            ViewBag.HeaderInfo = headerInfo;

            if(mode.Equals("compact", StringComparison.InvariantCultureIgnoreCase))
            {
                model.CompactView = true;
            }

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(BookingViewModel model)
        {
            var success = true;
            Exception exception = null;

            try
            {
                model.Booking.Services = model.Services.ToString();
                model.Booking.WorkingHours = model.CarerAvailability.ToString();

                if (model.IsNew)
                { 
                    bookingService.SaveBooking(model.Booking);
                } else
                {
                    var existingBooking = bookingService.GetBooking(model.Booking.ID);
                    if (existingBooking.CanAmendBooking())
                    {
                        existingBooking.LiveInRate = model.Booking.LiveInRate;
                        existingBooking.HourlyRate = model.Booking.HourlyRate;
                        existingBooking.HourlyRateWeekends = model.Booking.HourlyRateWeekends;
                        existingBooking.HourlyRateHolidays = model.Booking.HourlyRateHolidays;
                        existingBooking.ContractualRequirements = model.Booking.ContractualRequirements;
                        existingBooking.PromotionalCode = model.Booking.PromotionalCode;
                        existingBooking.PayerID = model.Booking.PayerID;
                        existingBooking.LockPayer = model.Booking.LockPayer;
                        existingBooking.CustomerID = model.Booking.CustomerID;
                        existingBooking.CareRecipientID = model.Booking.CareRecipientID;
                        existingBooking.CarerID = model.Booking.CarerID;
                        existingBooking.MaxWeeklyHours = model.Booking.MaxWeeklyHours;
                        existingBooking.Summary = model.Booking.Summary;
                        existingBooking.OriginalReference = model.Booking.OriginalReference;
                        existingBooking.Payer_Ref_No = model.Booking.Payer_Ref_No;
                        existingBooking.CareTypeID = model.Booking.CareTypeID;
                        existingBooking.EstimatedExpenses = model.Booking.EstimatedExpenses;
                        existingBooking.PaymentMethodID = model.Booking.PaymentMethodID;
                        existingBooking.CustomerCareRecipientRelationshipID = model.Booking.CustomerCareRecipientRelationshipID;
                        existingBooking.CCEmail1 = model.Booking.CCEmail1;
                        existingBooking.CCEmail2 = model.Booking.CCEmail2;
                        existingBooking.DDMandateNumber = model.Booking.DDMandateNumber;
                        existingBooking.Services = model.Booking.Services;
                        existingBooking.WorkingHours = model.Booking.WorkingHours;
                        existingBooking.Note = model.Booking.Note;
                        existingBooking.ExpensesNotes = model.Booking.ExpensesNotes;
                        existingBooking.Total = model.Booking.Total;

                        bookingService.SaveBooking(existingBooking);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
                exception = ex;
            }

            TempData["ShowNewCustomerModal"] = model.ShowNewCustomerModal;
            TempData["ShowNewPayerModal"] = model.ShowNewPayerModal;
            TempData["ShowNewCareRecipientModal"] = model.ShowNewCareRecipientModal;
            TempData["ShowNewNoteModal"] = model.ShowNewNoteModal;
            TempData["ShowBookingPreviewModal"] = model.ShowBookingPreviewModal;
            TempData["ShowStatusChangeModal"] = model.ShowStatusChangeModal;
            if (model.NewBookingStatus.HasValue)
            {
                TempData["NewBookingStatus"] = model.NewBookingStatus;
            }

            if (!model.ShowNewCustomerModal && !model.ShowNewPayerModal && !model.ShowNewCareRecipientModal && !model.ShowNewNoteModal && !model.ShowStatusChangeModal)
            {
                SetFeedbackMessage(success,
                    "The booking has been saved.",
                    "There has been an error whilst saving the booking.",
                    exception);
            }

            return RedirectToAction("Edit", new { id = model.Booking.ID });
        }

        #region Send Booking Preview

        public ActionResult ShowBookingPreviewEmailModal(int Id)
        {
            var booking = bookingService.GetBooking(Id);
            var body = renderingService.RenderHtml(booking,
                renderingService.GetTemplatePath(TemplateType.BOOKING_PREVIEW_EMAIL));

            var model = new RichTextContentViewModel()
            {
                Text = body
            };

            ViewBag.Id = Id;
            ViewBag.NextAction = "SendBookingPreviewEmail";

            return PartialView("_MessageModal", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendBookingPreviewEmail(RichTextContentViewModel model, int id)
        {
            var success = true;

            try
            {
                var booking = bookingService.GetBooking(id);

                messagingService.QueueEmail(booking.CustomerID.Value,
                    new MailAddress(booking.Customer.Email, booking.Customer.DisplayName),
                    null,
                    null,
                    ApplicationSettings.BookingPreviewEmailSubject,
                    model.Text,
                    null,
                    (new BookingsEmailAccount()).FromAddress);

                booking.BookingStatus = BookingStatus.WaitingForCustomerAcceptance;
                if (booking.BookingStatusChanged)
                {
                    booking.CurrentBookingStatus = new BookingStatusHistory()
                    {
                        BookingID = booking.ID,
                        Reason = "Booking preview sent to customer",
                        Notes = "",
                        BookingStatus = booking.BookingStatus,
                        CreatedBy = ServiceContext.User.ID,
                        CreatedDate = DateTime.Now
                    };

                    bookingService.SaveBooking(booking);
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The booking preview email is being sent to the customer.",
                "There has been an error whilst sending the booking preview email to the customer.");

            return RedirectToSamePage();
        }

        #endregion

        public ActionResult Clone(int id)
        {
            var success = true;
            Booking booking = null;

            try
            {
                booking = bookingService.Clone(id);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The booking has been cloned.",
                "There has been an error whilst cloning the booking.");

            if (success)
            {
                return RedirectToAction("Edit", new { id = booking.ID });
            }
            else
            {
                return RedirectToSamePage();
            }
        }

        #region AJAX

        public JsonResult SearchBookingsJSON(string search)
        {
            try
            {
                var bookings = bookingService
                    .SearchBookings(new Search { BookingReference = search})
                    .Select(x => new { ID = x.Reference.ToString(), Value = $"{x.Reference}" + (x.Customer == null?"":$" - {x.Customer?.FormattedName}") });

                return Json(new { items = bookings, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBookingDefaults(int carerId)
        {
            try
            {
                var carer = carerService
                    .GetCarer(carerId);

                var hourlyRate = carer.Profile.Find(x => x.Key == Common.Constants.ProfilePropertyName.HOURLY_RATE);
                var weeklyRate = carer.Profile.Find(x => x.Key == Common.Constants.ProfilePropertyName.WEEKLY_RATE);

                return Json(new { result = new {
                    HourlyRate = hourlyRate?.Value,
                    WeeklyRate = weeklyRate?.Value
                }, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Modals

        public ActionResult ViewModal(int Id)
        {
            var booking = bookingService.GetBooking(Id);
            var headerInfo = new Dictionary<string, string>();

            if (booking.CustomerID.HasValue)
            {
                headerInfo.Add("Customer", booking.Customer.FormattedName);
            }
            if (booking.CareRecipientID.HasValue)
            {
                headerInfo.Add("Care Recipient", booking.CareRecipient.FormattedName);
            }
            if (booking.CarerID.HasValue)
            {
                headerInfo.Add("Carer", booking.Carer.FormattedName);
            }

            headerInfo.Add("Created Date", booking.CreatedDate.ToString("dd/MM/yyyy HH:mm"));
            headerInfo.Add("Created By", booking.GetCreatedByUser()?.FormattedName);

            if (booking.UpdateDate.HasValue)
            {
                headerInfo.Add("Last Updated", booking.UpdateDate.Value.ToString("dd/MM/yyyy HH:mm"));
                headerInfo.Add("Updated By", booking.GetUpdatedByUser()?.FormattedName);
                if (booking.Timesheets.Any())
                {
                    headerInfo.Add("Number Of Timesheets", booking.Timesheets.Count.ToString());
                }
                if (booking.Invoices.Any())
                {
                    headerInfo.Add("Value Of Billings", booking.Invoices.Sum(x => x.Total).ToMoney());
                }
            }      

            ViewBag.HeaderInfo = headerInfo;

            return PartialView("_ViewModal", booking);
        }

        public ActionResult ChangeStatusModal(int id, int newStatus)
        {
            var newBookingStatus = new BookingStatusHistory()
            {
                BookingID = id,
                BookingStatusID = newStatus
            };

            var reasons = new List<SelectListItem>();

            bookingService.GetStatusReasons((BookingStatus)newStatus).ForEach(x => reasons.Add(new SelectListItem() { Text = x.Reason, Value = x.Reason }));
            ViewData["Reasons"] = reasons;
            return PartialView("_ChangeStatusModal", newBookingStatus);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChangeStatus(BookingStatusHistory model)
        {
            var success = true;

            try
            {
                var booking = bookingService.GetBooking(model.BookingID);
                booking.BookingStatus = model.BookingStatus;

                if (booking.BookingStatusChanged)
                { 
                    booking.CurrentBookingStatus = new BookingStatusHistory()
                    {
                        BookingID = model.BookingID,
                        Reason = model.Reason,
                        Notes = model.Notes,
                        BookingStatus = model.BookingStatus,
                        CreatedBy = ServiceContext.User.ID,
                        CreatedDate = DateTime.Now
                    };

                    bookingService.SaveBooking(booking);
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The booking status has been amended.",
                $"There has been an error whilst amending the booking status.");

            return RedirectToAction("Edit", new { id = model.BookingID });
        }


        public ActionResult SearchModal()
        {
            return PartialView("_SearchModal", new Search());
        }

        public ActionResult Search(Search search)
        {
            var bookings = bookingService.SearchBookings(search)
                .ToList();

            return View("SearchResults", bookings);
        }
        
        public ActionResult GetStandardWording(string category)
        {
            var wording = bookingService.GetStandardWording(category);
            return PartialView("_StandardWording", wording);
        }

        public ActionResult AddBookingNoteModal(int Id)
        {
            ViewBag.Id = Id;
            ViewBag.NextAction = "AddBookingNote";

            return PartialView("_NoteModal");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBookingNote(RichTextContentViewModel model, int id)
        {
            var success = true;

            try
            {
                if (!string.IsNullOrWhiteSpace(model.Text))
                {
                    bookingService.AddNote(id, model.Text);
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Booking note successfully added.",
                "There has been an error whilst adding the booking note.");

            TempData["SetFocus"] = "pnlNotes";

            return RedirectToAction("Edit", new { id = id });
        }
        #endregion


        public ActionResult GetBookingPartialView(int? bookingID, string viewName)
        {
            var model = new BookingViewModel();
            if (bookingID.HasValue)
            {
                model.Booking = bookingService.GetBooking(bookingID.Value);
            }
            return PartialView(viewName, model);
        }

        public FileResult PreviewBookingSummary(int id)
        {
            var booking = bookingService.GetBooking(id);
            var documentContent = bookingService.GenerateBookingSummary(booking);

            var mimeType = Utilities.GetMimeType(".pdf");
            var file = new FileContentResult(documentContent, mimeType)
            {
                FileDownloadName = $"Booking No {booking.Reference} {booking.Carer.DisplayName}.pdf"
            };

            return file;
        }
    }
}