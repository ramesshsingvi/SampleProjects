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
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services;

namespace TrustonTap.Web.Controllers
{
    public class BaseController : System.Web.Mvc.Controller
    {
        private ServiceContext context;

        public ServiceContext ServiceContext
        {
            get
            {
                if(context == null)
                {
                    context = new ServiceContext(TrustonTap_InternalDB.GetInstance(), SiteSqlServerDB.GetInstance())
                    {
                        User = GetCurrentUser()
                    };
                }
                return context;
            }
        }

        public bool IsPopup
        {
            get
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                if(actionName.Contains("Modal"))
                {
                    return true;
                }
                return false;
            }
        }

        private InternalUser currentUser;
        public InternalUser GetCurrentUser()
        {
            if (currentUser == null)
            {
                var authenticationManager = HttpContext.GetOwinContext().Authentication;
                var user = authenticationManager.User;

                var email = user.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();
                var userId = Convert.ToInt32(user.Claims.Where(c => c.Type == ClaimTypes.System).Select(c => c.Value).SingleOrDefault());
                var username = user.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var name = user.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

                currentUser = new InternalUser()
                {
                    ID = userId,
                    UserName = username,
                    Email = email,
                    Name = name
                };
            }
            return currentUser;
        }

        public new HttpContextBase HttpContext
        {
            get
            {
                HttpContextWrapper context =
                    new HttpContextWrapper(System.Web.HttpContext.Current);
                return (HttpContextBase)context;
            }
        }

        protected ActionResult RedirectToSamePage()
        {
            string url = this.Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }

        protected void SetFeedbackMessage(bool success, string successMessage = null, string failureMessage = null, Exception exception = null)
        {
            TempData["Success"] = success;

            if(success && !String.IsNullOrWhiteSpace(successMessage))
            {
                TempData["FeedbackMessage"] = $"<strong>Success!</strong> {successMessage}";
            }

            if (!success && !String.IsNullOrWhiteSpace(failureMessage))
            {
                TempData["FeedbackMessage"] = $"<strong>Error!</strong> {failureMessage}<!--{exception}-->";
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"];
            }

            if (TempData["FeedbackMessage"] != null)
            {
                ViewBag.FeedbackMessage = TempData["FeedbackMessage"];
            }

            base.OnActionExecuting(filterContext);
        }
    }
}