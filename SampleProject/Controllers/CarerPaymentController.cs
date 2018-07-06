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
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Carer Payments", "Index")]
    public class CarerPaymentController : BaseController
	{
        private IPaymentService paymentService;
        private IStatementService statementService;
        private ICarerService carerService;
        private ILoggingService loggingService;

        public CarerPaymentController(
            IPaymentService paymentService,
            IStatementService statementService,
            ICarerService carerService,
            ILoggingService loggingService)
        {
            this.paymentService = paymentService;
            this.statementService = statementService;
            this.carerService = carerService;
            this.loggingService = loggingService;
        }

        public ActionResult Index()
        {
            var statements = statementService.GetCarerStatementsByStatus(CarerStatementStatus.PartiallyPaid, CarerStatementStatus.SentToCarer);

            var headerInfo = new Dictionary<string, string>
            {
                { "Outstanding", statements.Sum(x => x.AmountOutstanding).ToMoney() },
                { "Total Selected", "<span id='totalSelected'>£0.00</span>" }
            };

            ViewBag.HeaderInfo = headerInfo;

            return View(statements);
        }

        public ActionResult PaymentHistory(int id)
        {
            var carer = carerService.GetCarer(id);
            var paymentHistory = paymentService.GetCarerBankTransactions(id).Where(x=>x.Successful).ToList();

            if (paymentHistory.Any())
            {
                var headerInfo = new Dictionary<string, string>
                {
                    { "Payments To Date", paymentHistory.Sum(x => Math.Abs(x.Amount)).ToMoney() },
                    { "Last Payment", paymentHistory.Max(x => x.PaymentDate).ToString("dd/MM/yyyy") }
                };

                ViewBag.HeaderInfo = headerInfo;
            }

            ViewBag.Carer = carer;

            return View(paymentHistory);
        }

        public ActionResult MakePaymentModal(int[] ids, string totalAmount)
        {
            ViewBag.TotalOutstanding = totalAmount;

            return PartialView("_MakePaymentModal", ids);
        }

        public ActionResult MakePayment(List<int> ids, string amount, string reference, int paymentMethod, string paymentDate, string notes)
        {
            var success = true;

            try
            {
                var date = DateTime.ParseExact(paymentDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                foreach (int statementId in ids)
                {
                    var statement = statementService.GetCarerStatement(statementId);

                    var paymentAmount = statementService
                        .GetCarerStatementsByCarerID(statement.CarerID)
                        .Where(x=>x.Status == CarerStatementStatus.SentToCarer)
                        .Sum(x=>x.AmountOutstanding);

                    paymentService.MakeCarerPayment(statement.CarerID, date, (PaymentMethod)paymentMethod, reference, paymentAmount, notes);
                }    
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been made.",
                "There has been an error whilst making the payment.");
            return RedirectToSamePage();
        }
    }
}