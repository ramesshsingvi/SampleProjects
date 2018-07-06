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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.JobService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.RenderingService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Services.JobService.Jobs;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Customer Payments", "Index")]
    public class CustomerPaymentController : BaseController
	{
        private IPaymentService paymentService;
        private IStatementService statementService;
        private IJobService jobService;
        private IRenderingService renderingService;
        private ILoggingService loggingService;
        private IUserService userService;

        public CustomerPaymentController(
            IPaymentService paymentService,
            IStatementService statementService,
            IJobService jobService,
            IRenderingService renderingService,
            IUserService userService,
            ILoggingService loggingService)
        {
            this.paymentService = paymentService;
            this.statementService = statementService;
            this.jobService = jobService;
            this.renderingService = renderingService;
            this.userService = userService;
            this.loggingService = loggingService;
        }

        public ActionResult Index(string Name)
        {
            var customers = paymentService
                .GetOutstandingDebtorsList(null)
                .Where(x => String.IsNullOrWhiteSpace(Name) || x.Key.DisplayName.ToLower().Contains(Name.ToLower()))
                .ToDictionary(x => x.Key, x => x.Value);

            var headerInfo = new Dictionary<string, string>
            {
                { "Balance", customers.Sum(x => x.Key.PaymentSummary.Balance).ToMoney() },
                { "Outstanding", customers.Sum(x => x.Key.PaymentSummary.Balance + x.Key.PaymentSummary.UnallocatedTransactions).ToMoney() },
                { "Unallocated", String.Format("<div style='color:red'>{0}</div>" , customers.Sum(x => x.Key.PaymentSummary.UnallocatedTransactions).ToMoney())}
            };

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Name = Name;

            return View(customers);
        }

        public ActionResult GoCardless()
        {
            var statements = statementService
                .GetCustomerStatementsByStatus(CustomerStatementStatus.PartiallyPaid, CustomerStatementStatus.SentToCustomer, CustomerStatementStatus.FailedPayment)
                .Where(x=>x.Booking.PaymentMethod == PaymentMethod.GoCardless)
                .ToList();

            var headerInfo = new Dictionary<string, string>
            {
                { "Outstanding", statements.Sum(x => x.AmountOutstanding).ToMoney() },
                { "Total Selected", "<span id='totalSelected'>£0.00</span>" }
            };

            ViewBag.HeaderInfo = headerInfo;
            return View(statements);
        }

        public ActionResult PayerPaymentHistory(int id)
        {
            var payer = userService.GetPayer(id);
            var paymentHistory = paymentService.GetPayerBankTransactions(id);
            ViewBag.Payer = payer;

            return View("PaymentHistory", paymentHistory);
        }

        public ActionResult CustomerPaymentHistory(int id)
        {
            var customer = userService.GetCustomer(id);
            var paymentHistory = paymentService.GetCustomerBankTransactions(id);
            ViewBag.Customer = customer;

            return View("PaymentHistory", paymentHistory);
        }

        public ActionResult Allocate(int id = 0)
        {
            var unallocatedTransactions = paymentService.GetPayerBankTransactions(id)
                .Where(x => x.AllocationStatus != AllocationStatus.FullyAllocated)
                .OrderBy(x=>x.Payer.DisplayName)
                .ToList();


            var headerInfo = new Dictionary<string, string>
            {
                { "Total", unallocatedTransactions.Sum(x=>x.AmountUnallocated).ToMoney()},
            };

            ViewBag.HeaderInfo = headerInfo;

            return View(unallocatedTransactions);
        }

        public ActionResult PaymentAllocationDetails(int id)
        {
            var transaction = paymentService.GetBankTransaction(id);
            var payments = paymentService.GetCustomerPayments(null, null, null, id, null, null, true)
                .GroupBy(x => new { x.OriginatingReference, x.CustomerStatementID })
                    .Select(e => new PaymentAllocationViewModel()
                    {
                        OriginatingReference = e.Key.OriginatingReference,
                        CustomerStatementID = e.Key.CustomerStatementID,
                        Allocated = e.Sum(x => x.PaymentReceipts.Where(y => y.TransactionID.Equals(transaction.ID)).Sum(z => z.Amount)),
                    }).ToList();

            var headerInfo = new Dictionary<string, string>
            {
                { "Reference", transaction.Reference },
                { "Amount", transaction.Amount.ToMoney() },
                { "Payment Method", transaction.PaymentMethod.GetDescription() },
                { "Payment Date", transaction.PaymentDate.ToShortDateString() },
                { "Total Outstanding", transaction.AmountUnallocated.ToMoney() }
            };

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Transaction = transaction;

            return View(payments);
        }

        #region Modals

        public ActionResult AllocationModal(int id)
        {
            var transaction = paymentService.GetBankTransaction(id);
            var statements = statementService.GetCustomerStatementsByPayerID(transaction.PayerID.Value)
                .Where(x => x.AmountOutstanding > 0)
                .OrderBy(x=>x.StatementDate)
                .ToList();

            var model = new PaymentAllocationModel()
            {
                Transaction = transaction
            };
            statements.ForEach(statement =>
            {
                model.Allocations.Add(statement, 0);
            });

            return PartialView("_AllocationModal", model);
        }


        [HttpPost]
        public ActionResult AllocatePayment(PaymentAllocationModel model)
        {
            var success = true;
            var failureMessage = "There has been an error whilst allocating the payment.";
            try
            {
                if (model.Allocations.Sum(x=>x.Value) > model.Amount)
                {
                    failureMessage += " The amount allocated to individual invoices cannot exceed the total payment amount.";
                    success = false;
                }
                else
                {
                    var transaction = paymentService.GetBankTransaction(model.Transaction.ID);
                    if (transaction != null)
                    {
                        foreach (var allocation in model.Allocations.Where(x => x.Value > 0))
                        {
                            paymentService.AllocatePayment(transaction, allocation.Value, allocation.Key.ID, model.Notes);
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
                "Payment has been allocated.",
                failureMessage);
            return RedirectToSamePage();
        }

        public ActionResult TransferUnallocatedModal(int id)
        {
            var transaction = paymentService.GetBankTransaction(id);

            var model = new TransferUnallocatedModel()
            {
                Transaction = transaction
            };

            return PartialView("_TransferModal", model);
        }

        public ActionResult TransferUnallocated(TransferUnallocatedModel model)
        {
            var success = true;
            var failureMessage = "There has been an error whilst refunding the payment.";
            try
            {
                var payment = paymentService.GetBankTransaction(model.Transaction.ID);
                if (payment != null)
                {
                    if (model.PayerID <= 0)
                    {
                        failureMessage += " You must select a payer to transfer to.";
                        success = false;
                    }
                    if (model.Amount <= 0)
                    {
                        failureMessage += " The amount to transfer must be more than £0.00.";
                        success = false;
                    }
                    else
                    {
                        paymentService.TransferCustomerPayment(payment, model.PayerID, model.Notes, model.Amount);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been transferred.",
                failureMessage);
            return RedirectToSamePage();
        }

        public ActionResult RefundModal(int id)
        {
            var transaction = paymentService.GetBankTransaction(id);

            return PartialView("_RefundModal", transaction);
        }

        [HttpPost]
        public ActionResult RefundPayment(decimal amount, int TransactionID, PaymentMethod paymentMethod, string reference, string notes)
        {
            var success = true;
            var failureMessage = "There has been an error whilst refunding the payment.";
            try
            {
                var payment = paymentService.GetBankTransaction(TransactionID);
                if (payment != null)
                {
                    if (amount > payment.AmountUnallocated)
                    {
                        failureMessage += " The amount to refund cannot exceed the total unallocated amount.";
                        success = false;
                    }
                    else
                    {
                        paymentService.RefundCustomerPayment(payment, DateTime.Now, paymentMethod, reference, notes, amount);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been refunded.",
                failureMessage);
            return RedirectToSamePage();
        }

        public ActionResult TakePaymentModal(int id)
        {
            var statements = statementService.GetCustomerStatementsByPayerID(id);
            var outstandingStatements = statements
                .Where(x => x.AmountOutstanding > 0)
                .OrderBy(x => x.StatementDate)
                .ToList();
            var payer = userService.GetPayer(id);

            var model = new TakePaymentViewModel()
            {
                Payer = payer,
                TotalOutstanding = outstandingStatements.Sum(x => x.AmountOutstanding)
            };

            outstandingStatements.ForEach(statement =>
            {
                model.Allocations.Add(statement, 0);
            });

            if (statements.Any())
            {
                model.PaymentMethod = statements.First().Booking.PaymentMethod;
            }

            return PartialView("_TakePaymentModal", model);
        }

        [HttpPost]
        public ActionResult TakePayment(TakePaymentViewModel model)
        {
            var success = true;

            var failureMessage = "There has been an error whilst taking the payment.";
            try
            {
                if (model.Allocations.Sum(x => x.Value) > model.Amount)
                {
                    failureMessage += " The amount allocated to individual invoices cannot exceed the total payment amount.";
                    success = false;
                }
                else
                {
                    var date = DateTime.ParseExact(model.PaymentDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var transaction = paymentService.TakeCustomerPayment(model.Payer.ID, date, model.PaymentMethod, model.Reference, model.Amount, model.Notes, true);

                    foreach (var allocation in model.Allocations.Where(x => x.Value > 0))
                    {
                        paymentService.AllocatePayment(transaction, allocation.Value, allocation.Key.ID, model.Notes);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been taken.",
                failureMessage);
            return RedirectToSamePage();
        }

        public ActionResult EditModal(int id)
        {
            var transaction = paymentService.GetBankTransaction(id);

            return PartialView("_EditPaymentModal", transaction);
        }

        [HttpPost]
        public ActionResult Edit(BankTransaction model)
        {
            var success = false;

            try
            {
                var transaction = paymentService.GetBankTransaction(model.ID);
                if (transaction != null)
                {
                    paymentService.UpdateTransaction(model.Amount, model.ID, model.PaymentDate, model.PaymentMethod, model.Notes, model.Reference);

                    success = true;
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been amended.",
                "There has been an error whilst amending the payment.");

            return RedirectToSamePage();
        }

        public ActionResult Delete(int id)
        {
            var success = false;

            try
            {
                var transaction = paymentService.GetBankTransaction(id);
                if (transaction != null)
                {
                    paymentService.DeleteTransaction(id);

                    success = true;
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment has been deleted.",
                "There has been an error whilst deleting the payment.");

            return RedirectToSamePage();
        }

        public ActionResult TakePaymentGoCardlessModal(int[] ids, string totalAmount)
        {
            ViewBag.TotalOutstanding = totalAmount;

            return PartialView("_TakePaymentGoCardlessModal", ids);
        }

        #endregion

        public ActionResult Details(int id)
        {
            var customer = userService.GetCustomer(id);
            var payments = paymentService.GetCustomerPayments(id, null, null, null, null, null, true);

            var headerInfo = new Dictionary<string, string>
            {
                { "Total", payments.Sum(x => x.Total).ToMoney() },
                { "Payments To Date", payments.Sum(x => x.PaymentReceipts.Sum(y=>y.Amount)).ToMoney() },
                { "Total Outstanding", payments.Sum(x => x.AmountOutstanding).ToMoney() }
            };

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Customer = customer;

            return View(payments);
        }

        [HttpPost]
        public FileResult ExportGoCardless(List<int> ids)
        {
            var statements = statementService
                .GetCustomerStatementsByStatus(CustomerStatementStatus.PartiallyPaid, CustomerStatementStatus.SentToCustomer, CustomerStatementStatus.FailedPayment)
                .Where(x => x.Agreement.PaymentProvider == PaymentProvider.GoCardless && ids.Contains(x.ID))
                .ToList();

            var statementData = new DataTable("GoCardless Payments");
            statementData.Columns.Add("Statement Ref");
            statementData.Columns.Add("Statement Date");
            statementData.Columns.Add("Payer ID");
            statementData.Columns.Add("Payer Name");
            statementData.Columns.Add("Amount").DataType = typeof(Decimal);

            statements.ForEach(statement =>
            {
                var row = statementData.NewRow();
                row["Statement Ref"] = statement.Reference;
                row["Statement Date"] = statement.StatementDate.ToShortDateString();
                row["Payer ID"] = statement.PayerID.ToString();
                row["Payer Name"] = statement.Agreement.Payer.FormattedName;
                row["Amount"] = statement.AmountOutstanding;

                statementData.Rows.Add(row);
            });

            var documentContent = renderingService.RenderExcel(statementData);
            var mimeType = Utilities.GetMimeType(".xlsx");
            var file = new FileContentResult(documentContent, mimeType)
            {
                FileDownloadName = "Export.xlsx"
            };

            return file;
        }

        [HttpPost]
        public ActionResult TakePaymentGoCardless(List<int> ids, string reference, string notes, string amount)
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<TakeGoCardlessPaymentsJob>(this.ServiceContext, new Dictionary<string, object>
                {
                    { "Statement Ids", ids },
                    { "Reference", reference ?? "" },
                    { "Notes", notes ?? "" }
                });
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "Payment requests have been submitted to GoCardless.",
                "There has been an error whilst taking the payment.");
            return RedirectToSamePage();
        }
    }
}