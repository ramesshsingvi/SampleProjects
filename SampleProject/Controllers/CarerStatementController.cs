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

using Autofac;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.JobService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.TimesheetService;
using TrustonTap.Services.JobService.Jobs;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Carer Payments", "Index")]
    public class CarerStatementController : BaseController
    {
        private IStatementService statementService;
        private ITimesheetService timesheetService;
        private IJobService jobService;
        private IPaymentService paymentService;
        private ICarerService carerService;
        private ILoggingService loggingService;
        private ILifetimeScope scope;

        public CarerStatementController(
            IStatementService statementService,
            ITimesheetService timesheetService,
            IJobService jobService,
            IPaymentService paymentService,
            ICarerService carerService,
            ILoggingService loggingService,
            ILifetimeScope scope)
        {
            this.statementService = statementService;
            this.paymentService = paymentService;
            this.jobService = jobService;
            this.carerService = carerService;
            this.timesheetService = timesheetService;
            this.loggingService = loggingService;
            this.scope = scope;
        }

        public ActionResult Index(int id = 0)
        {
            var statements = statementService.GetCarerStatementsByCarerID(id)
                .ToList();

            if (id > 0)
            {
                var carer = carerService.GetCarer(id);
                ViewBag.Carer = carer;
            }

            return View(statements);
        }

        public ActionResult Batches()
		{
            var statementBatches = statementService.GetStatementBatches()
                .Where(x => x.Type == StatementBatchType.CarerStatement)
                .ToList();

            ViewBag.PermitNewStatementRun = !statementBatches
                .Where(x => x.Status == StatementBatchStatus.BatchCreated ||
                    x.Status == StatementBatchStatus.StatementsGenerated ||
                    x.Status == StatementBatchStatus.StatementsGenerated)
                .Any();

            return View(statementBatches);
		}

        public ActionResult BatchDetails(int id)
        {
            var batch = statementService.GetStatementBatch(id);
            var statements = statementService.GetCarerStatements()
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
                headerInfo.Add("Total Outstanding", batch.TotalOutstanding.ToMoney());
            }

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Batch = batch;

            return View(statements);
        }

        public ActionResult StatementDetails(int id)
        {
            var statement = statementService.GetCarerStatement(id);
            var timesheetRefs = statement.Payments.Select(x => x.OriginatingReference).Distinct().ToList();

            var model = new List<TimesheetWeek>();
            timesheetRefs.ForEach(timesheetRef => {
                var timesheet = timesheetService.GetTimesheetByReference(timesheetRef);
                if (timesheet != null)
                {
                    model.Add(timesheet);
                }
                });

            var headerInfo = new Dictionary<string, string>
            {
                { "Invoice Date", statement.CreatedDate.ToString("g") },
                { "Reference", statement.Reference},
                { "Batch", String.Format("<a href='{0}'>{1}</a>", Url.Action("BatchDetails", "CarerStatement", new { ID = statement.BatchID }), statement.Batch.Reference) },
                { "Invoice Total", statement.Payments.Sum(x => x.Total).ToMoney()},
            };

            if (statement.Status != CarerStatementStatus.Created)
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

            ViewBag.Status = statement.Status.GetDescription();
            ViewBag.Carer = statement.Carer;
            ViewBag.HeaderInfo = headerInfo;

            return View(model);
        }

        public ActionResult SendCarerStatementsModal(int Id)
        {
            ViewBag.Id = Id;
            ViewBag.NextAction = "SendCarerStatements";

            return PartialView("_MessageModal");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendCarerStatements(RichTextContentViewModel model, int id)
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob< SendCarerStatementsJob>(this.ServiceContext, new Dictionary<string, object>
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
                "The statements are being sent. Please <a href =\"javascript: location.reload();\">refresh</a> the page in a few minutes.",
                "There has been an error whilst sending the statements.");
            return RedirectToAction("Batches");
        }

        public ActionResult GenerateStatements()
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<GenerateCarerStatementsJob>(this.ServiceContext);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The statements are being generated. Please <a href=\"javascript: location.reload();\">refresh</a> the page in a few minutes.",
                "There has been an error whilst generating the statements.");
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
            var statements = statementService.SearchCarerStatements(search)
                .ToList();

            return View("SearchResults", statements);
        }
    }
}