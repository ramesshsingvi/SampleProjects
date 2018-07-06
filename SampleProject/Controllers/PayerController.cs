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
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.BookingService;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.TimesheetService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Payers", "Index")]
    public class PayerController : BaseController
	{
        private IPaymentService paymentService;
        private IStatementService statementService;
        private ITimesheetService timesheetService;
        private ICarerService carerService;
        private ILoggingService loggingService;
        private IBookingService bookingService;
        private IUserService userService;

        public PayerController(
            IPaymentService paymentService,
            IStatementService statementService,
            ITimesheetService timesheetService,
            ICarerService carerService,
            ILoggingService loggingService,
            IBookingService bookingService,
            IUserService userService)
        {
            this.paymentService = paymentService;
            this.statementService = statementService;
            this.timesheetService = timesheetService;
            this.carerService = carerService;
            this.loggingService = loggingService;
            this.bookingService = bookingService;
            this.userService = userService;
        }

        public ActionResult Index()
        {
            return this.RedirectToAction("Search", "Payer");
        }

        public ActionResult Details(int id)
        {
            User payer = userService.GetPayer(id);

            var payments = paymentService.GetPayerBankTransactions(id);

            ViewBag.Payments = payments;

            return View(payer);
        }

        public RedirectResult Impersonate(int id)
        {
            return new RedirectResult($"{ApplicationSettings.PublicWebsiteUrl}/my-trustontap?userid={id}");
        }

        public ActionResult Search(PayerSearchViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Name))
            {
                var payers = userService.SearchPayers(model.Name)
                    .OrderBy(x => x.DisplayName)
                    .ToList();

                model.Results = payers;
            }

            return View(model);
        }

        public JsonResult SearchPayersJSON(string search)
        {
            try
            {
                var payers = userService
                    .SearchPayers(search)
                    .Select(x=> new { ID = x.ID.ToString(), Value = x.DisplayName });

                return Json(new { items = payers, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}