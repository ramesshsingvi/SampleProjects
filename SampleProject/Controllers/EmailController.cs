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
using System.Text;
using System.Web;
using System.Web.Mvc;
using TrustonTap.Common.Services.MessagingService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    public class EmailController : BaseController
    {
        private IMessagingService messagingService;

        public EmailController(IMessagingService messagingService)
        {
           this. messagingService = messagingService;
        }

        public ActionResult Index()
        {
            var email = messagingService.GetEmails();
            return View(email);
        }

        public ActionResult View(int id)
        {
            var email = messagingService.GetEmail(id);

            var headerInfo = new Dictionary<string, string>
            {
                { "To", HttpUtility.HtmlEncode(email.To) },
                { "Cc", HttpUtility.HtmlEncode(email.Cc) },
                { "Subject", email.Subject }
            };

            if (email.Attachments.Any())
            {
                StringBuilder attachments = new StringBuilder();
                foreach (var attachment in email.Attachments)
                {
                    attachments.AppendFormat("<a href = '{0}'><i class='fa fa-download fa-lg'></i>&nbsp;{1}</a><br />", Url.Action("DownloadDocument", "Document", new { Id = attachment.Document.ID }), attachment.Document.ReferenceNo);
                }
                headerInfo.Add("Attachments", attachments.ToString());
            }

            if (email.SentDate.HasValue)
            {
                headerInfo.Add("Sent", email.SentDate.Value.ToShortDateString());
            }

            ViewBag.HeaderInfo = headerInfo;

            return View(email);
        }
    }
}