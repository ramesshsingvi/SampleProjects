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
using System.Linq;
using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.BookingService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.MessagingService;
using TrustonTap.Common.Services.RenderingService;
using TrustonTap.Common.Services.UserService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Manage Users", "Index")]
    public class UserController : BaseController
    {
        private ILoggingService loggingService;
        private IRenderingService renderingService;
        private IMessagingService messagingService;
        private IUserService userService;
        private IBookingService bookingService;

        public UserController(
            ILoggingService loggingService,
            IMessagingService messagingService,
            IRenderingService renderingService,
            IUserService userService,
            IBookingService bookingService)
        {
          
            this.loggingService = loggingService;
            this.messagingService = messagingService;
            this.renderingService = renderingService;
            this.userService = userService;
            this.bookingService = bookingService;
        }

        public ActionResult Index()
        {
            var users = userService.SearchUsers("");

            return View(users);
        }

        #region AJAX
        
        public JsonResult SearchUsersJSON(string search)
        {
            try
            {
                var users = userService
                    .SearchUsers(search)
                    .Select(x => new { ID = x.ID.ToString(), Value = $"{x.DisplayName} [{x.ID}] {Utilities.FormatAddress(false, new string[] { x.AddressLine1, x.Postcode })}" });

                return Json(new { items = users, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUserJSON(int id)
        {
            try
            {
                var user = userService
                    .GetUser(id);

                return Json(new { result = new { name = $"{user.DisplayName} [{user.ID}] {Utilities.FormatAddress(false, new string[] { user.AddressLine1, user.Postcode })}", id=user.ID.ToString() }, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggingService.LogException(ex);

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Modals
        

        public ActionResult NewUserModal(int? bookingId, UserType userType, string title)
        {
            var model = new NewUserViewModel()
            {
                User = new User(),
                BookingId = bookingId,
                Title = title,
                UserType = userType
            };

            return PartialView("_NewUserModal", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewUser(NewUserViewModel model)
        {
            var success = true;
            var errorMessage = "";
            Booking booking = null;

            try
            {
                if (model.BookingId.HasValue)
                {
                    booking = bookingService.GetBooking(model.BookingId.Value);
                    model.User.SourceBooking = booking.Reference;
                }

                if(model.GenerateUsername)
                {
                    var random3digit = (new Random()).Next(100, 1000);
                    model.User.Username = $"{model.User.FirstName}_{model.User.LastName}_{random3digit}";
                }
                else
                {
                    model.User.Username = model.User.Email;
                }

                var newPassword = Utilities.CreatePassword(12);
                model.User.Password = newPassword;
                var newUserResponse = userService.CreateUser(model.User);

                if (booking != null && newUserResponse.User.ID > 0)
                {
                    var bookingChanged = false;
                    switch (model.UserType)
                    {
                        case UserType.Customer:
                            userService.AddUserToRole(newUserResponse.User.ID, Roles.CUSTOMER);
                            booking.CustomerID = newUserResponse.User.ID;
                            booking.InitialCustomerPassword = newPassword;
                            bookingChanged = true;
                            break;
                        case UserType.CareRecipient:
                            booking.CareRecipientID = newUserResponse.User.ID;
                            bookingChanged = true;
                            break;
                        case UserType.Payer:
                            booking.PayerID = newUserResponse.User.ID;
                            bookingChanged = true;
                            break;
                        default:
                            break;
                    }
                    if (bookingChanged)
                    {
                        bookingService.SaveBooking(booking);
                    }
                }
                else
                {
                    errorMessage = newUserResponse.ErrorMessage;
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The {model.UserType.GetDescription()} {model.User.FormattedName} has been added.",
                $"There has been an error whilst adding the {model.UserType.GetDescription()} {model.User.FormattedName}. {errorMessage}");

            if (booking != null)
            {
                return RedirectToAction("Edit", "Booking", new { id = model.BookingId });
            } else
            {
                return RedirectToSamePage();
            }
        }
        #endregion

    }
}