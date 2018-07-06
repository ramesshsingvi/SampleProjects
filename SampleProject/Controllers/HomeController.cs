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
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.TimesheetService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Home", "Index")]
    public class HomeController : BaseController
    {
        private IPaymentService paymentService;
        private ITimesheetService timesheetService;
        private IStatementService statementService;
        private ILoggingService loggingService;

        public HomeController(
            IPaymentService paymentService,
            ITimesheetService timesheetService,
            IStatementService statementService,
            ILoggingService loggingService)
        {
            this.paymentService = paymentService;
            this.timesheetService = timesheetService;
            this.statementService = statementService;
            this.loggingService = loggingService;
        }

        public ActionResult Index()
		{
            try
            {
                ViewBag.CarerPaymentTotal = paymentService.GetCarerPayments(null, null, null, null).Sum(x => x.AmountOutstanding).ToMoney();
                ViewBag.CustomerPaymentTotal = paymentService.GetCustomerPayments(null, null, null, null, null, null, false).Sum(x => x.AmountOutstanding).ToMoney();
                ViewBag.TimesheetCount = timesheetService.GetTimesheets(null, null, null, false, null, null, null, null).Count();
                ViewBag.GoCardlessTotal = statementService
                    .GetCustomerStatementsByStatus(CustomerStatementStatus.PartiallyPaid, CustomerStatementStatus.SentToCustomer, CustomerStatementStatus.FailedPayment)
                    .Where(x => x.Booking.PaymentMethod == PaymentMethod.GoCardless)
                    .Sum(x => x.AmountOutstanding).ToMoney();
            }
            catch(Exception ex)
            {
                loggingService.LogException(ex);
            }

            return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}