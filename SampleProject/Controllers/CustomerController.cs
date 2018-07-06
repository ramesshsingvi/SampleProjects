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
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.PaymentService;
using TrustonTap.Common.Services.StatementService;
using TrustonTap.Common.Services.TimesheetService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Customers", "Index")]
    public class CustomerController : BaseController
	{
        private IPaymentService paymentService;
        private IStatementService statementService;
        private ITimesheetService timesheetService;
        private ICarerService carerService;
        private ILoggingService loggingService;
        private IUserService userService;

        public CustomerController(
            IPaymentService paymentService,
            IStatementService statementService,
            ITimesheetService timesheetService,
            ICarerService carerService,
            IUserService userService,
            ILoggingService loggingService)
        {
            this.paymentService = paymentService;
            this.statementService = statementService;
            this.timesheetService = timesheetService;
            this.carerService = carerService;
            this.userService = userService;
            this.loggingService = loggingService;
        }

        public ActionResult Index()
        {
            return this.RedirectToAction("Search", "Customer");
        }

        public ActionResult Details(int id)
        {
            User customer = userService.GetCustomer(id);
            if(customer == null)
            {
                customer = userService.GetPayer(id);
            }
            if (customer == null)
            {
                customer = userService.GetUser(id);
            }

            var payments = paymentService.GetCustomerPayments(id, null, null, null, null, null, true);
            var timesheets = timesheetService.GetTimesheets(null, id, null, null, null, null, null);

            var headerInfo = new Dictionary<string, string>
            {
                { "Payments To Date", @payments.Sum(x => x.AmountPaid).ToMoney() },
                { "Outstanding", @payments.Sum(x => x.AmountOutstanding).ToMoney() },
                { "Invoices To Date", timesheets.Sum(x => x.Summary.InvoiceValue.HasValue ? x.Summary.InvoiceValue.Value : 0).ToMoney() },
                { "Address", Utilities.FormatAddress(true,
                        customer.AddressLine1,
                        customer.AddressLine2,
                        customer.AddressLine3,
                        customer.Town,
                        customer.Postcode) },
                { "Telephone", customer.Phone },
                { "Email", $"<a href=\"mailto:{customer.Email}\">{customer.Email}</a>"},
            };

            ViewBag.HeaderInfo = headerInfo;
            ViewBag.Timesheets = timesheets;
            ViewBag.Payments = payments;

            return View(customer);
        }

        public RedirectResult Impersonate(int id)
        {
            return new RedirectResult($"{ApplicationSettings.PublicWebsiteUrl}/my-trustontap?userid={id}");
        }

        public ActionResult Search(CustomerSearchViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Name))
            {
                var customers = userService.SearchCustomers(model.Name)
                    .OrderBy(x => x.DisplayName)
                    .ToList();

                model.Results = customers;
            }

            return View(model);
        }
    }
}