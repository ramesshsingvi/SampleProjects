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
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.JobService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Services.JobService.Jobs;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Customer Invoices", "Batches")]
    public class CustomerInvoiceController : BaseController
    {
        private IStatementService statementService;
        private IJobService jobService;
        private IUserService userService;
        private ILoggingService loggingService;

        public CustomerInvoiceController(
            IStatementService statementService,
            IJobService jobService,
            IUserService userService,
            ILoggingService loggingService)
        {
            this.statementService = statementService;
            this.jobService = jobService;
            this.userService = userService;
            this.loggingService = loggingService;
        }

        public ActionResult Index(int id = 0)
        {
            var statements = statementService.GetCustomerStatementsByPayerID(id)
                .ToList();

            if (id > 0)
            {
                var payer = userService.GetPayer(id);
                ViewBag.Payer = payer;
            }

            return View(statements);
        }

        public ActionResult Batches()
        {
            var statementBatches = statementService.GetStatementBatches()
                .Where(x => x.Type == StatementBatchType.CustomerStatement)
                .ToList();

            ViewBag.PermitNewInvoiceRun = !statementBatches
                .Where(x => x.Status == StatementBatchStatus.BatchCreated ||
                    x.Status == StatementBatchStatus.StatementsGenerated ||
                    x.Status == StatementBatchStatus.StatementsGenerated)
                .Any();

            return View(statementBatches);
        }

        public ActionResult Cancel(int id)
        {
            var success = true;

            try
            {
                statementService.CancelInvoice(id);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The invoice has been cancelled.",
                "There has been an error whilst cancelling the invoice.");
            return RedirectToSamePage();
        }

        public ActionResult Unallocate(int id)
        {
            var success = true;

            try
            {
                statementService.UnallocateInvoicePayments(id);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "All payments against this invoice have been unallocated.",
                "There has been an error whilst unallocating payments against this invoice.");
            return RedirectToSamePage();
        }

        public ActionResult BatchDetails(int id)
        {
            var batch = statementService.GetStatementBatch(id);

            var statements = statementService.GetCustomerStatements()
                .Where(x => x.BatchID == id)
                .ToList();

            var headerInfo = new Dictionary<string, string>
            {
                { "Date", batch.CreatedDate.ToString("g") },
                { "Invoice Batch Total", batch.BatchValue.ToMoney() }
            };

            if (batch.PostedDate.HasValue)
            {
                headerInfo.Add("Date Posted", batch.PostedDate.Value.ToString("g"));
                headerInfo.Add("Value Still To Be Paid", batch.TotalOutstanding.ToMoney());
            }

            if (statements.Any(x => x.Status == CustomerStatementStatus.Cancelled))
            {
                headerInfo.Add("Cancelled Invoices", batch.CancelledValue.ToMoney());
                headerInfo.Add("Revised Batch Total", batch.RevisedBatchValue.ToMoney());
            }

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Batch = batch;

            return View(statements);
        }

        public ActionResult SendCustomerInvoicesModal(int Id)
        {
            ViewBag.Id = Id;
            ViewBag.NextAction = "SendCustomerInvoices";

            return PartialView("_MessageModal");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendCustomerInvoices(RichTextContentViewModel model, int id)
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<SendCustomerStatementsJob>(this.ServiceContext, new Dictionary<string, object>
                {
                    { "Batch Id", id },
                    { "Message", model.Text??"" }
                });
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The invoices are being sent. Please <a href =\"javascript: location.reload();\">refresh</a> the page in a few minutes.",
                "There has been an error whilst sending the invoices.");
            return RedirectToAction("Batches");
        }

        public ActionResult InvoiceDetails(int id)
        {
            var statement = statementService.GetCustomerStatement(id);
            if (statement == null)
            {
                return HttpNotFound();
            }

            var headerInfo = new Dictionary<string, string>
            {
                { "Invoice Date", statement.StatementDate.ToShortDateString() },
                { "Carer", $"<a href='/Carer/Details/{statement.Booking.CarerID}'>{statement.Booking.Carer.DisplayName}</a>"},
                { "Care Recipient", statement.Booking.CareRecipient?.DisplayName},
                { "Invoice Total", statement.Payments.Sum(x => x.Total).ToMoney()},
                { "Reference", String.Format("<a href='{0}'>{1}</a>", Url.Action("ViewByReference", "Timesheet", new { Reference = statement.Reference }), statement.Reference) },
                { "Batch", String.Format("<a href='{0}'>{1}</a>", Url.Action("BatchDetails", "CustomerInvoice", new { ID = statement.BatchID }), statement.Batch.Reference) },
            };

            if (statement.Status != CustomerStatementStatus.Created)
            {
                headerInfo.Add("Outstanding", statement.Payments.Sum(x => x.AmountOutstanding).ToMoney());
            };

            if (statement.DocumentID.HasValue)
            {
                headerInfo.Add("Invoice", String.Format("<a href='{0}'><i class='fa fa-download fa-lg'></i>&nbsp;{1}</a>", Url.Action("DownloadDocument", "Document", new { Id = statement.DocumentID }), statement.Document.ReferenceNo));
            };

            if (statement.EmailID.HasValue)
            {
                headerInfo.Add("Email", String.Format("<a href='{0}'><i class='fa fa-envelope-o fa-lg'></i>&nbsp;{1}</a>", Url.Action("View", "Email", new { Id = statement.EmailID }), statement.EmailMessage.Subject));
            };

            if (statement.Timesheet.HoldFromCarerStatement)
            {
                headerInfo.Add("<i title='Held From Carer Statement' style='color:red' class='fa fa-exclamation-triangle'></i> Held From Carer Statement", "&nbsp;");
            };

            ViewBag.HeaderInfo = headerInfo;

            return View(statement);
        }

        public ActionResult GenerateInvoices()
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<GenerateCustomerStatementsJob>(this.ServiceContext);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The invoices are being generated. Please <a href=\"javascript: location.reload();\">refresh</a> the page in a few minutes.",
                "There has been an error whilst generating the invoices.");
            return RedirectToAction("Batches");
        }

        public ActionResult DeleteBatch(int id)
        {
            var success = true;

            try
            {
                statementService.DeleteBatch(id);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The batch has been deleted",
                "The batch could not be deleted.");
            return RedirectToAction("Batches");
        }


        public ActionResult SearchModal()
        {
            return PartialView("_SearchModal", new Search());
        }

        public ActionResult Search(Search search)
        {
            var statements = statementService.SearchCustomerStatements(search)
                .ToList();

            return View("SearchResults", statements);
        }
    }
}