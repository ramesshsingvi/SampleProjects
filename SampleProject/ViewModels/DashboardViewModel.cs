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
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Web.Controllers;

namespace TrustonTap.Web.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel(List<CustomerStatement> customerInvoices, List<CarerStatement> carerStatements, List<TimesheetWeek> timesheets, List<TimesheetWeek> timesheetsAwaitingInvoice)
        {
            var thisWeek = DateTime.Now.Date.GetWeekEndDate();
            this.ThisWeek = new DashboardValues(thisWeek)
            {
                CustomerInvoices = customerInvoices.Where(x => x.CreatedDate.Date.GetWeekEndDate() == thisWeek).ToList(),
                CarerStatements = carerStatements.Where(x => x.CreatedDate.Date.GetWeekEndDate() == thisWeek).ToList(),
                CarerStatementsPaid = carerStatements.Where(x => x.DatePaid.HasValue && x.DatePaid.Value.Date.GetWeekEndDate() == thisWeek).ToList(),

                TimesheetsSubmitted = timesheets
                    .Where(x => x.WeekEnding == thisWeek && (x.Status == TimesheetStatus.Approved || x.Status == TimesheetStatus.Submitted))
                    .ToList(),

                TimesheetsRejected = timesheets
                    .Where(x => x.WeekEnding == thisWeek && x.Status == TimesheetStatus.Rejected)
                    .ToList(),

                TimesheetsSaved = timesheets
                    .Where(x => x.WeekEnding == thisWeek && x.Status == TimesheetStatus.Created)
                    .ToList(),

                 TimesheetsAwaitingInvoice = timesheetsAwaitingInvoice
                    .Where(x => x.WeekEnding == thisWeek)
                    .ToList()
            };

            var lastWeek = DateTime.Now.Date.AddDays(-7).GetWeekEndDate();
            this.LastWeek = new DashboardValues(lastWeek)
            {
                CustomerInvoices = customerInvoices.Where(x => x.CreatedDate.Date.GetWeekEndDate() == lastWeek).ToList(),
                CarerStatements = carerStatements.Where(x => x.CreatedDate.Date.GetWeekEndDate() == lastWeek).ToList(),
                CarerStatementsPaid = carerStatements.Where(x => x.DatePaid.HasValue && x.DatePaid.Value.Date.GetWeekEndDate() == lastWeek).ToList(),
                TimesheetsSubmitted = timesheets
                    .Where(x => x.WeekEnding == lastWeek && (x.Status == TimesheetStatus.Approved || x.Status == TimesheetStatus.Submitted))
                    .ToList(),

                TimesheetsRejected = timesheets
                    .Where(x => x.WeekEnding == lastWeek && x.Status == TimesheetStatus.Rejected)
                    .ToList(),

                TimesheetsSaved = timesheets
                    .Where(x => x.WeekEnding == lastWeek && x.Status == TimesheetStatus.Created)
                    .ToList(),

                TimesheetsAwaitingInvoice = timesheetsAwaitingInvoice
                    .Where(x => x.WeekEnding == lastWeek)
                    .ToList()
            };


            this.Older = new DashboardValues(lastWeek)
            {
                CustomerInvoices = customerInvoices.Where(x => x.CreatedDate.Date.GetWeekEndDate() < lastWeek).ToList(),
                CarerStatements = carerStatements.Where(x => x.CreatedDate.Date.GetWeekEndDate() < lastWeek).ToList(),
                CarerStatementsPaid = carerStatements.Where(x => x.DatePaid.HasValue && x.DatePaid.Value.Date.GetWeekEndDate() < lastWeek).ToList(),
                TimesheetsSubmitted = timesheets
                    .Where(x => x.WeekEnding < lastWeek && (x.Status == TimesheetStatus.Approved || x.Status == TimesheetStatus.Submitted))
                    .ToList(),

                TimesheetsRejected = timesheets
                    .Where(x => x.WeekEnding < lastWeek && x.Status == TimesheetStatus.Rejected)
                    .ToList(),

                TimesheetsSaved = timesheets
                    .Where(x => x.WeekEnding < lastWeek && x.Status == TimesheetStatus.Created)
                    .ToList(),

                TimesheetsAwaitingInvoice = timesheetsAwaitingInvoice
                    .Where(x => x.WeekEnding < lastWeek)
                    .ToList()
            };
        }



        public List<TimesheetWeek> TimesheetsOnHold { get; set; }

        public List<CustomerStatement> CustomerInvoicesOnHold { get; set; }

        public List<CustomerStatement> CustomerInvoicesGeneratedNotSent { get; set; }

        public List<CustomerStatement> CustomerInvoicesDue { get; set; }

        public List<BankTransaction> UnallocatedPayments { get; set; }

        public DashboardValues ThisWeek { get; set; }

        public DashboardValues LastWeek { get; set; }

        public DashboardValues Older { get; set; }

        public List<CustomerStatement> CustomerInvoicesPaidAwaitingCarerInvoice { get; set; }

        public List<CarerStatement> CarerStatementsGeneratedNotSent { get; set; }

        public List<CarerStatement> CarerStatementsGeneratedAwaitingPayment { get; set; }
    }

    public class DashboardValues
    {
        private DateTime weekEnding;
        public DashboardValues(DateTime weekEnding)
        {
            this.weekEnding = weekEnding;
        }

        public DateTime WeekEnding
        {
            get
            {
                return this.weekEnding;
            }
        }

        public List<CustomerStatement> CustomerInvoices { get; set; }

        public List<CarerStatement> CarerStatements { get; set; }

        public List<CarerStatement> CarerStatementsPaid { get; set; }

        public List<CustomerStatement> Invoiced
        {
            get
            {
                return CustomerInvoices.ToList();
            }
        }

        public List<TimesheetWeek> TimesheetsSaved { get; set; }

        public List<TimesheetWeek> TimesheetsAwaitingInvoice { get; set; }

        public List<TimesheetWeek> TimesheetsSubmitted { get; set; }

        public List<TimesheetWeek> TimesheetsRejected { get; set; }
    }
}