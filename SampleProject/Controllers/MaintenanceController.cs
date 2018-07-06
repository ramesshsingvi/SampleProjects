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

using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using TrustonTap.Common.Services.JobService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Services.JobService.Jobs;
using TrustonTap.Web.Models;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Maintenance", "Index")]
    public class MaintenanceController : BaseController
    {
        private IJobService jobService;
        private ILoggingService loggingService;

        public MaintenanceController(IJobService jobService, ILoggingService loggingService)
        {
            this.jobService = jobService;
            this.loggingService = loggingService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Reset()
        {
            var success = true;

            try
            {
                var context = this.ServiceContext;

                if (context.IsTestEnvironment)
                {
                    jobService.ExecuteBackgroundJob<ResetDatabaseJob>(this.ServiceContext);
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The payment system has been reset. All data has been removed",
                $"There has been an error whilst resetting the payment system.");

            return RedirectToAction("Index");
        }

        public ActionResult Backup()
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<BackupDatabaseJob>(this.ServiceContext);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The payment system is being backed up",
                $"There has been an error whilst backing up the payment system.");

            return RedirectToAction("Index");
        }

        public ActionResult RunZohoSync()
        {
            var success = true;

            try
            {
                jobService.ExecuteBackgroundJob<SyncToZohoJob>(this.ServiceContext, new Dictionary<string, object> { { "Start Date", DateTime.Now.AddDays(-7) } });
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The Zoho syncronization feed has been activated.",
                $"There has been an error whilst running the Zoho syncronization feed.");

            return RedirectToAction("Index");
        }

        public ActionResult Status()
        {
            var applicationStatus = GetApplicationStatus();

            var headerInfo = new Dictionary<string, string>
            {
                { "Sent Emails", applicationStatus.SentEmails.ToString() },
                { "Emails Waiting To Be Sent", applicationStatus.UnSentEmails.ToString() },
                { "Successful Batches", applicationStatus.SuccessfulBatches.ToString() },
                { "Batches In Progress", applicationStatus.BatchesInProgress.ToString() },
                { "Failed Payments", applicationStatus.FailedPayments.ToString() },
                { "Unsubmitted Timesheets", applicationStatus.UnsubmittedTimesheets.ToString() },
            };

            ViewBag.HeaderInfo = headerInfo;

            return View("Status");
        }

        private ApplicationStatus GetApplicationStatus()
        {
            var database = ServiceContext.ExternalWebsiteDatabaseContext.DatabaseName;

            var sql = Sql.Builder.Append($@"SELECT
                    SentEmails = (SELECT count(1) from tot.EmailMessage WHERE SentDate is not null),
                    UnSentEmails = (SELECT count(1) from tot.EmailMessage WHERE SentDate is null),
	                FailedPayments = (SELECT count(1) FROM tot.BankTransaction where Successful = 0),
	                SuccessfulBatches = (SELECT count(1) FROM tot.StatementBatch where StatusID not in (2, 4, 6)),
	                BatchesInProgress = (SELECT count(1) FROM tot.StatementBatch where StatusID  in (2, 4, 6)),
	                UnsubmittedTimesheets = (SELECT count(1) FROM {database}.tot.TimesheetWeek where TimesheetStatusID  in (1, 3, 4))"
                );

            return ServiceContext.InternalDatabaseContext.FirstOrDefault<ApplicationStatus>(sql);
        }


    }
}