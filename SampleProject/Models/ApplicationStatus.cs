using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrustonTap.Web.Models
{
    public class ApplicationStatus
    {
        public int SentEmails { get; set; }
        public int UnSentEmails { get; set; }
        public int FailedPayments { get; set; }
        public int SuccessfulBatches { get; set; }
        public int BatchesInProgress { get; set; }
        public int UnsubmittedTimesheets { get; set; }
    }
}