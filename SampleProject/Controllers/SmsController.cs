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
using TrustonTap.Common.Services.CarerService;
using TrustonTap.Common.Services.LoggingService;
using TrustonTap.Common.Services.MessagingService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Sms", "Index")]
    public class SmsController : BaseController
    {
        // GET: Sms
        private IMessagingService messagingService;
        private ILoggingService loggingService;
        private ICarerService carerService;
        public SmsController(
            IMessagingService messagingService,
            ILoggingService loggingService,
            ICarerService carerService)
        {
            this.messagingService = messagingService;
            this.loggingService = loggingService;
            this.carerService = carerService;
        }

        [HttpGet]
        public ActionResult Index(string phone, int carerId, bool isPopup = true, SmsResponseSource src = SmsResponseSource.Unknown)
        {
            SmsViewModel model = new SmsViewModel();
            string name = "";
            if (carerId > 0)
            {
                var carer = carerService.GetCarer(carerId);
                if (carer != null)
                {
                    name = carer.FormattedName;
                }
                model.UserId = carerId;
            }
            if (src == SmsResponseSource.Unknown)
            {
                if (!IsPopup)
                    src = SmsResponseSource.Recruitment;
                else
                    src = SmsResponseSource.Support;
            }

            model.Name = name;
            model.ToNumber = phone;
            model.StandardResponses = GetStandardResponses(src);
            model.IsPopup = isPopup;
            model.ResponseSource = src;

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SmsViewModel model)
        {
            bool success = false;

            //gives weird error of selectListItem vs String
            ModelState.Remove("StandardResponses");

            if (ModelState.IsValid)
            {
                try
                {
                    if (model != null && !string.IsNullOrWhiteSpace(model.ToNumber) && !string.IsNullOrWhiteSpace(ApplicationSettings.TwilioPhoneNum) && !string.IsNullOrWhiteSpace(model.Message))
                    {
                        var currentUser = GetCurrentUser();
                        var sms = new SmsMessage
                        {
                            FromNumber = ApplicationSettings.TwilioPhoneNum,
                            ToNumber = model.ToNumber.Trim(),
                            UserId = model.UserId,
                            Message = model.Message,
                            ReplyReceived = false,
                            SentDate = DateTime.Now,
                            CreatedById = currentUser.ID,
                            SmsResponseSource = model.ResponseSource
                        };

                        messagingService.SendSms(sms);
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    loggingService.LogException(ex);
                }

                SetFeedbackMessage(success,
                    "Sms has been sent successfully.",
                    "There has been an error whilst sending sms. Please try again.");

                return RedirectToAction("Index", new { phone=model.ToNumber, carerId = model.UserId, isPopup = model.IsPopup, src = model.ResponseSource });
            }
            else
            {
                model.StandardResponses = GetStandardResponses(model.ResponseSource);
                return View(model);
            }
        }

        #region Private Methods

        private List<SelectListItem> GetStandardResponses(SmsResponseSource responseSource)
        {
            var responses = new List<SelectListItem>(){
                new SelectListItem {
                    Text = "--- Select response ---",
                    Value = ""
                }
            };

            responses.AddRange(messagingService.GetStandardSmsResponses(responseSource).Select(r => new SelectListItem
            {
                Text = r.ResponseTitle,
                Value = r.ResponseMessage
            }));

            return responses;
        }

        #endregion
    }
}