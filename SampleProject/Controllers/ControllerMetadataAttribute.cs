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

namespace TrustonTap.Web.Controllers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerMetadataAttribute : ActionFilterAttribute
    {
        private string defaultActionName = "Index";

        public string FriendlyName { get; set; }
        public string DefaultAction { get; set; }

        public ControllerMetadataAttribute(string friendlyName) : base()
        {
            FriendlyName = friendlyName;
        }

        public ControllerMetadataAttribute(string friendlyName, string defaultAction) : this(friendlyName)
        {
            DefaultAction = defaultAction;
        }

        public static string GetFriendlyName<T>(T controller) where T:ControllerBase
        {
            var type = controller.GetType();
            var attribute = type.GetCustomAttributes(typeof(ControllerMetadataAttribute), true).FirstOrDefault() as ControllerMetadataAttribute;

            if(attribute == null)
            {
                return type.Name.Replace("Controller", "");
            }

            return attribute.FriendlyName;
        }

        public static string GetDefaultAction<T>(T controller) where T : ControllerBase
        {
            var type = controller.GetType();
            var attribute = type.GetCustomAttributes(typeof(ControllerMetadataAttribute), true).FirstOrDefault() as ControllerMetadataAttribute;

            if (attribute == null)
            {
                return null;
            }

            return attribute.DefaultAction;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool indexActionRequested = filterContext.HttpContext.Request.Url.Segments
                .Select(x=>x.TrimEnd('/').ToUpper()).Contains(defaultActionName.ToUpper());
            if (!string.IsNullOrWhiteSpace(DefaultAction) && !indexActionRequested)
            {
                string currentAction = filterContext.RequestContext.RouteData.Values["action"].ToString();
                if (string.Compare(defaultActionName, currentAction, true) == 0)
                {
                    filterContext.RequestContext.RouteData.Values["action"] = DefaultAction;
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}