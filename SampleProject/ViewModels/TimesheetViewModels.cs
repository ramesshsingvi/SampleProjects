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

using System.Collections.Generic;
using TrustonTap.Common.Models;

namespace TrustonTap.Web.ViewModels
{
    public class TimesheetWeekViewModel : TimesheetWeek
    {
        public bool ShowExpensesModal { get; set; }

        public bool ShowSummaryModal { get; set; }

        public new List<TimesheetDayViewModel> Days { get; set; }

        public static TimesheetWeekViewModel ToViewModel(TimesheetWeek timesheet)
        {
            var viewModel = new TimesheetWeekViewModel()
            {
                Notes = timesheet.Notes,
                Status = timesheet.Status,
                Reference = timesheet.Reference,
                WeekEnding = timesheet.WeekEnding,
                CarerID = timesheet.CarerID,
                AgreementID = timesheet.AgreementID,
                CarerPaymentGenerated = timesheet.CarerPaymentGenerated,
                CreatedDate = timesheet.CreatedDate,
                CustomerPaymentGenerated = timesheet.CustomerPaymentGenerated,
                ID = timesheet.ID,
                TimesheetStatusID = timesheet.TimesheetStatusID,
                SubmittedBy = timesheet.SubmittedBy,
                SubmittedDate = timesheet.SubmittedDate,
                Days = new List<TimesheetDayViewModel>()
            };

            foreach (TimesheetDay day in timesheet.Days)
            {
                viewModel.Days.Add(TimesheetDayViewModel.ToViewModel(day));
            }

            return viewModel;
        }
    }

    public class TimesheetDayViewModel : TimesheetDay
    {
        public new List<TimesheetPeriodViewModel> Periods { get; set; }
        public static TimesheetDayViewModel ToViewModel(TimesheetDay timesheetDay)
        {
            var viewModel = new TimesheetDayViewModel()
            {
                CreatedDate = timesheetDay.CreatedDate,
                ID = timesheetDay.ID,
                Date = timesheetDay.Date,
                Notes = timesheetDay.Notes,
                TimesheetWeekID = timesheetDay.TimesheetWeekID,
                Periods = new List<TimesheetPeriodViewModel>()
            };

            foreach (TimesheetPeriod period in timesheetDay.Periods)
            {
                viewModel.Periods.Add(TimesheetPeriodViewModel.ToViewModel(period));
            }
            return viewModel;
        }
    }
    public class TimesheetPeriodViewModel : TimesheetPeriod
    {
        public string NumberOfHours { get; set; }

        public static TimesheetPeriodViewModel ToViewModel(TimesheetPeriod timesheetPeriod)
        {
            var viewModel = new TimesheetPeriodViewModel()
            {
                CreatedDate = timesheetPeriod.CreatedDate,
                ID = timesheetPeriod.ID,
                AutomaticallyAllocatedFromBulkTime = timesheetPeriod.AutomaticallyAllocatedFromBulkTime,
                EndTime = timesheetPeriod.EndTime,
                StartTime = timesheetPeriod.StartTime,
                TimesheetDayID = timesheetPeriod.TimesheetDayID,
            };

            return viewModel;
        }

    }
}