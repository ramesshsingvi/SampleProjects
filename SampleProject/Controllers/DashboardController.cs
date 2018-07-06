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
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Dashboard")]
    public class DashboardController : BaseController
    {
        private IPaymentService paymentService;
        private IStatementService statementsService;

        public DashboardController(IPaymentService paymentService, IStatementService statementsService)
        {
            this.paymentService = paymentService;
            this.statementsService = statementsService;
        }

        public ActionResult Index()
        {
            var lastWeek = DateTime.Now.Date.AddDays(-7).GetWeekStartDate();

            // Timesheets
            var sql = Sql.Builder.Append("SELECT * FROM tot.TimesheetWeek with(nolock)");
            var timesheets = ServiceContext.ExternalWebsiteDatabaseContext.Query<TimesheetWeek>(sql).ToList();

            sql = Sql.Builder.Append(@"select * from tot.TimesheetWeek TW
                      LEFT JOIN tot.TimesheetSummary TS ON TW.ID = TS.timesheetID
                      WHERE TS.CustomerStatementID IS NULL AND TS.Status not in ('Saved', 'Rejected')");
            var timesheetsAwaitingInvoice = ServiceContext.ExternalWebsiteDatabaseContext.Query<TimesheetWeek>(sql).ToList();

            // Timesheets On Hold
            sql = Sql.Builder.Append("SELECT * FROM tot.TimesheetWeek with(nolock) WHERE HoldFromCustomerInvoice = 1 OR HoldFromCarerStatement = 1");
            var timesheetsOnHold = ServiceContext.ExternalWebsiteDatabaseContext.Query<TimesheetWeek>(sql).ToList();
            var invoiceReferencesOnHold = timesheetsOnHold.Where(x => x.HoldFromCarerStatement).Select(x => x.Reference).ToList();

            // Customer Invoices
            sql = Sql.Builder.Append("SELECT * FROM tot.CustomerStatement with(nolock) WHERE CreatedDate >= @0 ", lastWeek);
            var invoices = ServiceContext.InternalDatabaseContext.Query<CustomerStatement>(sql).ToList();

            var invoicesOnHold = new List<CustomerStatement>();
            if (invoiceReferencesOnHold.Count > 0)
            {
                sql = Sql.Builder.Append($"SELECT * FROM tot.CustomerStatement with(nolock) WHERE Reference IN ({string.Join(",", invoiceReferencesOnHold.Select(x => $"'{x}'"))})");
                invoicesOnHold.AddRange(ServiceContext.InternalDatabaseContext.Query<CustomerStatement>(sql));
            }

            sql = Sql.Builder.Append("SELECT * FROM tot.CustomerStatement with(nolock) WHERE StatementStatusID = @0", CustomerStatementStatus.Created);
            var invoicesGeneratedNotSent = ServiceContext.InternalDatabaseContext.Query<CustomerStatement>(sql).ToList();

            // Carer Statements
            sql = Sql.Builder.Append("SELECT * FROM tot.CarerStatement with(nolock) WHERE CreatedDate >= @0", lastWeek);
            var statements = ServiceContext.InternalDatabaseContext.Query<CarerStatement>(sql).ToList();

            sql = Sql.Builder.Append("SELECT * FROM tot.CarerStatement with(nolock) WHERE StatementStatusID = @0", CarerStatementStatus.Created);
            var statementGeneratedNotSent = ServiceContext.InternalDatabaseContext.Query<CarerStatement>(sql).ToList();


            sql = Sql.Builder.Append($"SELECT * FROM tot.CarerStatement with(nolock) WHERE StatementStatusID in ({(int)CarerStatementStatus.SentToCarer}, {(int)CarerStatementStatus.PartiallyPaid})");
            var statementsGeneratedAwaitingPayment = ServiceContext.InternalDatabaseContext.Query<CarerStatement>(sql).ToList();

            var unallocatedTransactions = paymentService.GetPayerBankTransactions(0)
                .Where(x => x.AllocationStatus != AllocationStatus.FullyAllocated)
                .ToList();

            var invoicesDue = statementsService.GetCustomerStatementsByStatus(CustomerStatementStatus.PartiallyPaid, CustomerStatementStatus.SentToCustomer)
                .OrderBy(x=>x.Timesheet.WeekEnding)
                .ToList();

            var customerInvoicesPaidAwaitingCarerInvoice = statementsService.GetCustomerStatementsByStatus(CustomerStatementStatus.FullyPaid);

            var model = new DashboardViewModel(invoices, statements, timesheets, timesheetsAwaitingInvoice)
            {
                TimesheetsOnHold = timesheetsOnHold.Where(x => x.HoldFromCustomerInvoice).ToList(),
                CustomerInvoicesOnHold = invoicesOnHold,
                CustomerInvoicesGeneratedNotSent = invoicesGeneratedNotSent,
                CarerStatementsGeneratedNotSent = statementGeneratedNotSent,
                CarerStatementsGeneratedAwaitingPayment = statementsGeneratedAwaitingPayment,
                UnallocatedPayments = unallocatedTransactions,
                CustomerInvoicesDue = invoicesDue,
                CustomerInvoicesPaidAwaitingCarerInvoice = customerInvoicesPaidAwaitingCarerInvoice
            };

            return View(model);
        }
	}
}