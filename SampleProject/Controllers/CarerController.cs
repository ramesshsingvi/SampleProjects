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
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Services.BookingService;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.MessagingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.TimesheetService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Carers", "Index")]
    public class CarerController : BaseController
	{
        private IPaymentService paymentService;
        private IBookingService bookingService;
        private IStatementService statementService;
        private ITimesheetService timesheetService;
        private ICarerService carerService;
        private ILoggingService loggingService;
        private IMessagingService messagingService;

        public CarerController(
            IPaymentService paymentService,
            IStatementService statementService,
            ITimesheetService timesheetService,
            IBookingService bookingService,
            ICarerService carerService,
            ILoggingService loggingService,
            IMessagingService messagingService)
        {
            this.paymentService = paymentService;
            this.statementService = statementService;
            this.timesheetService = timesheetService;
            this.carerService = carerService;
            this.loggingService = loggingService;
            this.messagingService = messagingService;
            this.bookingService = bookingService;
        }

        public ActionResult Index()
        {
            var carers = paymentService.GetOutstandingCreditorsList();

            var headerInfo = new Dictionary<string, string>
            {
                { "Payments To Date", carers.Sum(x => x.Key.PaymentSummary.PaymentsToDate).ToMoney() },
                { "Outstanding", carers.Sum(x => x.Key.PaymentSummary.Balance).ToMoney() },
            };

            ViewBag.HeaderInfo = headerInfo;

            return View(carers);
        }

        public ActionResult Details(int id)
        {
            var carer = carerService.GetCarer(id);
            var payments = paymentService.GetCarerPayments(id, null, null, null);
            var timesheets = timesheetService.GetTimesheets(id, null, null, null, null, null, null, null);
            var bookings = bookingService.GetBookingsForCarer(id);
            var messages = messagingService.GetSmsMessagesByUser(id);

            var availabilityLastUpdated = carerService.GetCarerAvailability(id).Max(x => x.UpdatedDate)?.ToString("dd/MM/yyyy");
            var headerInfo = new Dictionary<string, string>
            {
                { "Address", Utilities.FormatAddress(true,
                        carer.AddressLine1,
                        carer.AddressLine2,
                        carer.AddressLine3,
                        carer.Town,
                        carer.Postcode)},

                { "Telephone", carer.Phone },
                { "Email", $"<a href=\"mailto:{carer.Email}\">{carer.Email}</a>"},
                {"Payments To Date", payments.Sum(x => x.AmountPaid).ToMoney() },
                {"Outstanding", payments.Sum(x => x.AmountOutstanding).ToMoney() },
                {"Availability Last Updated", String.IsNullOrEmpty(availabilityLastUpdated) ? "Never" : availabilityLastUpdated }
            };

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Bookings = bookings;
            ViewBag.Timesheets = timesheets;
            ViewBag.SmsMessages = messages;

            return View(carer);
        }

        public ActionResult Availability(int id)
        {
            var carerHoliday = carerService.GetCarerHoliday(id);
            if (carerHoliday == null)
            {
                carerHoliday = new Common.Models.CarerHoliday()
                {
                    CarerID = id
                };
            }

            var model = new AvailabilityViewModel()
            {
                Carer = carerService.GetCarer(id),
                CarerAvailability = carerService.GetCarerAvailability(id),
                CarerHoliday = carerHoliday
            };

            var headerInfo = new Dictionary<string, string>
            {
                { "Last Updated", model.CarerAvailability.Max(x=>x.UpdatedDate)?.ToString("dd/MM/yyyy") }
            };

            ViewBag.HeaderInfo = headerInfo;

            return View(model);
        }

        public RedirectResult Impersonate(int id)
        {
            return new RedirectResult($"{ApplicationSettings.PublicWebsiteUrl}/my-trustontap?userid={id}");
        }

        [HttpPost]
        public ActionResult Availability(AvailabilityViewModel model)
        {
            var success = true;
            try
            {
                model.CarerAvailability.ForEach(x => x.UpdatedByAdmin = true);
                carerService.SaveCarerAvailability(model.CarerAvailability);
                carerService.SaveCarerHoliday(model.CarerHoliday);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Availability has been saved.",
                "There has been an error whilst saving the availability.");

            return RedirectToSamePage();
        }

        public ActionResult Search(CarerSearchViewModel model)
        {
            model.Results = carerService.SearchCarers(model);

            return View(model);
        }

        public JsonResult SearchCarersJSON(string search)
        {
            try
            {
                var carers = carerService
                    .SearchCarers(search)
                    .Select(x => new { ID = x.ID.ToString(), Value = $"{x.DisplayName} [{x.ID}] {Utilities.FormatAddress(false, new string[] { x.AddressLine1, x.Postcode })}" });

                return Json(new { items = carers, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCarerJSON(int id)
        {
            try
            {
                var carer = carerService
                    .GetCarer(id);

                return Json(new { result = carer, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}