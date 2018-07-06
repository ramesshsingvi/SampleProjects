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

namespace TrustonTap.Web.ViewModels
{
    public class BookingViewModel
    {
        public BookingViewModel() : this(new Booking())
        {
        }

        public BookingViewModel(Booking booking)
        {
            Booking = booking;
            Services = Services.FromString(booking.Services);
            CarerAvailability = WorkingHours.FromString(booking.WorkingHours);
        }

        public Booking Booking { get; set; }

        public BookingStatus? NewBookingStatus { get; set; }

        public Services Services { get; set; }

        public WorkingHours CarerAvailability { get; set; }

        public bool IsNew { get { return (this.Booking == null || this.Booking.ID == 0); } }

        public bool IsFormFilledIn
        {
            get
            {
                return !ValidationErrors.Any();
            }
        }

        private List<string> validationErrors;
        public List<string> ValidationErrors
        {
            get
            {
                if (validationErrors == null)
                {
                    validationErrors = new List<string>();
                    if (Booking != null && this.Booking.ID > 0)
                    {
                        validationErrors = Booking.Validate();
                    }
                }
                return validationErrors;
            }
        }

        public bool ShowNewCustomerModal { get; set; }
        public bool ShowNewCareRecipientModal { get; set; }
        public bool ShowNewPayerModal { get; set; }
        public bool ShowNewNoteModal { get; set; }
        public bool ShowBookingPreviewModal { get; set; }
        public bool ShowStatusChangeModal { get; set; }
        public bool CompactView { get; set; }
    }

    public class WorkingHours : List<CarerAvailability>
    {
        public WorkingHours(IEnumerable<CarerAvailability> collection) : base(collection)
        { }

        public WorkingHours() : base()
        {}

        public override string ToString()
        {
            var ignore = new List<KeyValuePair<Type, string>>();
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "ID"));
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "CarerID"));
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "UpdatedDate"));
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "UpdatedBy"));
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "UpdatedByAdmin"));
            ignore.Add(new KeyValuePair<Type, string>(typeof(CarerAvailability), "CreatedDate"));

            return Utilities.SerializeToXmlString(this, false, ignore);
        }

        public static WorkingHours FromString(string workingHoursString)
        {
            try
            {
                if (String.IsNullOrEmpty(workingHoursString))
                {
                    var workingHours = new WorkingHours();
                    var daysOfTheWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().OrderBy(x => ((int)x + 6) % 7);
                    foreach (DayOfWeek dayOfWeek in daysOfTheWeek)
                    {
                        workingHours.Add(new CarerAvailability(dayOfWeek) { });
                    }
                    return workingHours;
                }
                else
                    return Utilities.DeserializeFromXmlString<WorkingHours>(workingHoursString);
            }
            catch(Exception)
            {
                return new WorkingHours();
            }
        }
    }
    public class Services
    {
        private const string GENERAL_HOME_HELP = "General Home Help";
        private const string PERSONAL_CARE = "Personal Care & Hygiene";
        private const string FOOD_PREPARATION = "Food Preparation & Shopping";
        private const string DEMENTIA = "Dementia Support";
        private const string GARDENING = "Gardening & House Maintenance";
        private const string TRANSPORT = "Transportation & Errands";
        private const string ADMIN = "Paperwork & Administration";

        public bool HomeHelp { get; set; }
        public bool Personal { get; set; }
        public bool Food { get; set; }
        public bool Dementia { get; set; }
        public bool Gardening { get; set; }
        public bool Transport { get; set; }
        public bool Admin { get; set; }

        public override string ToString()
        {
            var services = new List<string>();
            if (HomeHelp)
                services.Add(GENERAL_HOME_HELP);
            if (Personal)
                services.Add(PERSONAL_CARE);
            if (Food)
                services.Add(FOOD_PREPARATION);
            if (Dementia)
                services.Add(DEMENTIA);
            if (Gardening)
                services.Add(GARDENING);
            if (Transport)
                services.Add(TRANSPORT);
            if (Admin)
                services.Add(ADMIN);

            return string.Join(", ", services);
        }

        public static Services FromString(string servicesString)
        {
            var services = new Services();

            if (!string.IsNullOrWhiteSpace(servicesString))
            {

                if (servicesString.Contains(GENERAL_HOME_HELP))
                    services.HomeHelp = true;

                if (servicesString.Contains(PERSONAL_CARE))
                    services.Personal = true;

                if (servicesString.Contains(FOOD_PREPARATION))
                    services.Food = true;

                if (servicesString.Contains(DEMENTIA))
                    services.Dementia = true;

                if (servicesString.Contains(GARDENING))
                    services.Gardening = true;

                if (servicesString.Contains(TRANSPORT))
                    services.Transport = true;

                if (servicesString.Contains(ADMIN))
                    services.Admin = true;
            }

            return services;
        }
    }
}